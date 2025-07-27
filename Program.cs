using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Tiktoken;
using System;
using System.Net.Http;
using System.Threading;
using System.IO;

namespace LLMApiConcurrentTest;

public class Config
{
    [YamlMember(Alias = "concurrent_num")]
    public int ConcurrentNum { get; set; }
    
    [YamlMember(Alias = "baseurl")]
    public string BaseUrl { get; set; } = string.Empty;
    
    [YamlMember(Alias = "api_key")]
    public string ApiKey { get; set; } = string.Empty;
    
    [YamlMember(Alias = "model_name")]
    public string ModelName { get; set; } = string.Empty;
    
    [YamlMember(Alias = "timeoutsec")]
    public int TimeoutSec { get; set; } = 10;
    
    [YamlMember(Alias = "language")]
    public string Language { get; set; } = "cn";
}

public static class LocalizationManager
{
    private static readonly Dictionary<string, Dictionary<string, string>> _translations = new()
    {
        ["cn"] = new Dictionary<string, string>
        {
            ["starting_test"] = "开始测试，使用 {0} 个{1}请求",
            ["concurrent"] = "并发",
            ["api_url"] = "API地址: {0}",
            ["model"] = "模型: {0}",
            ["timeout"] = "超时时间: {0} 秒 (故意设置较短以确保在AI回复完成前中断，保证并发测试数据准确性)",
            ["test_results"] = "🎯 测试结果:",
            ["question"] = "问题: {0}",
            ["first_response_time"] = "首次响应时间: {0:F2} ms",
            ["output_speed"] = "输出速度: {0:F2} Token/s",
            ["statistics_summary"] = "📊 统计摘要:",
            ["successful_requests"] = "成功请求数: {0}/{1}",
            ["avg_first_response"] = "平均首次响应时间: {0:F2} ms",
            ["avg_output_speed"] = "平均输出速度: {0:F2} Token/s",
            ["total_output_tokens"] = "总计输出Token: {0:N0}",
            ["total_token_output_speed"] = "总计Token输出速度: {0:F2} Token/s",
            ["no_successful_requests"] = "❌ 没有成功的请求完成",
            ["api_call_failed"] = "API调用失败，状态码: {0}",
            ["api_call_exception"] = "API调用异常: {0}",
            ["token_calculation_warning"] = "Token计算警告: {0}，使用备用估算方法",
            ["invalid_api_url"] = "API地址格式不正确",
            ["concurrent_num_range"] = "并发数量必须在1-20之间",
            ["timeout_range"] = "超时时间必须在1-120秒之间",
            ["model_validation_failed"] = "模型验证失败: {0}",
            ["available_models"] = "可用的模型列表 ({0} 个):",
            ["suggested_similar_models"] = "建议的相似模型:",
            ["model_not_found_in_list"] = "配置的模型 '{0}' 在API返回的模型列表中未找到",
            ["validation_direct_call"] = "未找到模型列表，尝试直接调用chat/completions接口进行验证...",
            ["model_validation_success"] = "模型验证成功（直接调用方式）",
            ["validation_failed_response"] = "验证失败响应内容: {0}",
            ["model_validation_failed_status"] = "模型 '{0}' 验证失败，状态码: {1}",
            ["model_validation_exception"] = "验证模型时发生异常: {0}",
            ["timeout_collecting_results"] = "测试超时，正在收集已完成的结果...",
            ["no_completed_requests"] = "没有完成的请求"
        },
        ["en"] = new Dictionary<string, string>
        {
            ["starting_test"] = "Starting test with {0} {1}request{2}",
            ["concurrent"] = "concurrent ",
            ["api_url"] = "API URL: {0}",
            ["model"] = "Model: {0}",
            ["timeout"] = "Timeout: {0} seconds (intentionally short to interrupt before AI completes response, ensuring accurate concurrent test data)",
            ["test_results"] = "🎯 Test Results:",
            ["question"] = "Question: {0}",
            ["first_response_time"] = "First response time: {0:F2} ms",
            ["output_speed"] = "Output speed: {0:F2} Token/s",
            ["statistics_summary"] = "📊 Statistics Summary:",
            ["successful_requests"] = "Successful requests: {0}/{1}",
            ["avg_first_response"] = "Average first response time: {0:F2} ms",
            ["avg_output_speed"] = "Average output speed: {0:F2} Token/s",
            ["total_output_tokens"] = "Total output tokens: {0:N0}",
            ["total_token_output_speed"] = "Total token output speed: {0:F2} Token/s",
            ["no_successful_requests"] = "❌ No successful requests completed",
            ["api_call_failed"] = "API call failed, status code: {0}",
            ["api_call_exception"] = "API call exception: {0}",
            ["token_calculation_warning"] = "Token calculation warning: {0}, using fallback estimation method",
            ["invalid_api_url"] = "Invalid API URL format",
            ["concurrent_num_range"] = "Concurrent number must be between 1 and 20",
            ["timeout_range"] = "Timeout must be between 1 and 120 seconds",
            ["model_validation_failed"] = "Model validation failed: {0}",
            ["available_models"] = "Available models list ({0} models):",
            ["suggested_similar_models"] = "Suggested similar models:",
            ["model_not_found_in_list"] = "Configured model '{0}' was not found in the API's model list",
            ["validation_direct_call"] = "Model list not found, trying direct call to chat/completions endpoint for validation...",
            ["model_validation_success"] = "Model validation successful (direct call method)",
            ["validation_failed_response"] = "Validation failed response content: {0}",
            ["model_validation_failed_status"] = "Model '{0}' validation failed, status code: {1}",
            ["model_validation_exception"] = "Exception occurred while validating model: {0}",
            ["timeout_collecting_results"] = "Test timeout, collecting completed results...",
            ["no_completed_requests"] = "No completed requests"
        }
    };
    
    private static string _currentLanguage = "cn";
    
    public static void SetLanguage(string language)
    {
        _currentLanguage = language.ToLower();
        if (!_translations.ContainsKey(_currentLanguage))
        {
            _currentLanguage = "cn"; // fallback to Chinese
        }
    }
    
    public static string Get(string key, params object[] args)
    {
        if (_translations.TryGetValue(_currentLanguage, out var languageDict) &&
            languageDict.TryGetValue(key, out var template))
        {
            return args.Length > 0 ? string.Format(template, args) : template;
        }
        
        // Fallback to key if not found
        return key;
    }
}

public class ChatCompletionRequest
{
    [JsonProperty("model")]
    public string Model { get; set; } = string.Empty;

    [JsonProperty("messages")]
    public List<Message> Messages { get; set; } = new();

    [JsonProperty("stream")]
    public bool Stream { get; set; } = true;
}

public class Message
{
    [JsonProperty("role")]
    public string Role { get; set; } = string.Empty;

    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty;
}

public class TestResult
{
    public string Question { get; set; } = string.Empty;
    public double FirstTokenTime { get; set; } // ms
    public double TokensPerSecond { get; set; }
    public int TotalTokens { get; set; } // 用于内部计算，不在输出中显示
}

public class ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly Config _config;

    public ApiClient(Config config)
    {
        _config = config;
        _httpClient = new HttpClient();
        
        if (!string.IsNullOrEmpty(config.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
        }
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<(bool isValid, string errorMessage)> ValidateModelAsync()
    {
        try
        {
            var baseUrl = _config.BaseUrl?.TrimEnd('/') ?? "";
            
            using var validateClient = new HttpClient();
            var response = await validateClient.GetAsync($"{baseUrl}/models");
            
            if (!response.IsSuccessStatusCode)
            {
                response = await _httpClient.GetAsync($"{baseUrl}/models");
                if (!response.IsSuccessStatusCode)
                {
                    return (false, LocalizationManager.Get("api_call_failed", response.StatusCode));
                }
            }
            
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var modelsResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse ?? "");
            
            if (modelsResponse != null && modelsResponse.ContainsKey("data") && modelsResponse["data"] is JArray modelsArray)
            {
                var availableModels = new List<string>();
                
                foreach (var modelToken in modelsArray)
                {
                    if (modelToken is JObject modelObj && 
                        modelObj.TryGetValue("id", out var idToken) && 
                        idToken?.Type == JTokenType.String)
                    {
                        var modelId = idToken.ToString();
                        availableModels.Add(modelId);
                        
                        if (string.Equals(modelId, _config.ModelName, StringComparison.Ordinal))
                        {
                            return (true, "");
                        }
                    }
                }
                
                Console.WriteLine(LocalizationManager.Get("available_models", availableModels.Count));
                for (int i = 0; i < availableModels.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {availableModels[i]}");
                }
                Console.WriteLine();
                
                var modelNameLower = (_config.ModelName ?? "").ToLower();
                var suggestedModels = availableModels
                    .Where(m => m.ToLower().Contains(modelNameLower) || 
                               modelNameLower.Contains(m.ToLower()))
                    .Take(3)
                    .ToList();
                
                if (suggestedModels.Any())
                {
                    Console.WriteLine(LocalizationManager.Get("suggested_similar_models"));
                    foreach (var suggested in suggestedModels)
                    {
                        Console.WriteLine($"  - {suggested}");
                    }
                    Console.WriteLine();
                }
                
                return (false, LocalizationManager.Get("model_not_found_in_list", _config.ModelName ?? "unknown"));
            }
            
            Console.WriteLine(LocalizationManager.Get("validation_direct_call"));
            
            var testRequest = new ChatCompletionRequest
            {
                Model = _config.ModelName ?? "",
                Messages = new List<Message>
                {
                    new Message { Role = "user", Content = "Hello" }
                },
                Stream = false
            };
            
            var json = JsonConvert.SerializeObject(testRequest);
            var content = new StringContent(json ?? "", Encoding.UTF8, "application/json");
            
            var testResponse = await _httpClient.PostAsync($"{baseUrl}/chat/completions", content);
            if (testResponse.IsSuccessStatusCode)
            {
                Console.WriteLine(LocalizationManager.Get("model_validation_success"));
                return (true, "");
            }
            
            var errorContent = await testResponse.Content.ReadAsStringAsync();
            Console.WriteLine(LocalizationManager.Get("validation_failed_response", errorContent));
            
            return (false, LocalizationManager.Get("model_validation_failed_status", _config.ModelName ?? "unknown", testResponse.StatusCode));
        }
        catch (Exception ex)
        {
            return (false, LocalizationManager.Get("model_validation_exception", ex.Message));
        }
    }

    public async Task<TestResult> SendRequestAsync(string question, CancellationToken cancellationToken)
    {
        var result = new TestResult { Question = question ?? "" };
        
        var request = new ChatCompletionRequest
        {
            Model = _config.ModelName ?? "",
            Messages = new List<Message>
            {
                new Message { Role = "user", Content = question ?? "" }
            },
            Stream = true
        };
        
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json ?? "", Encoding.UTF8, "application/json");
        
        HttpRequestMessage? httpRequest = null;
        HttpResponseMessage? response = null;
        
        var startTime = DateTime.UtcNow;
        var firstTokenTime = 0.0;
        var tokenCount = 0;
        var responseContent = new StringBuilder();
        
        try
        {
            httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_config.BaseUrl?.TrimEnd('/')}/chat/completions")
            {
                Content = content
            };
            
            response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(LocalizationManager.Get("api_call_failed", response.StatusCode));
            }
            
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            
            var firstContentTokenReceived = false;
            var endTime = startTime.AddSeconds(_config.TimeoutSec);
            
            while (!cancellationToken.IsCancellationRequested && DateTime.UtcNow < endTime)
            {
                try
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) break;
                    
                    if (line.StartsWith("data: ") && line.Length > 6)
                    {
                        var data = line.Substring(6).Trim();
                        if (data == "[DONE]") break;
                        
                        try
                        {
                            var jsonObject = JObject.Parse(data);
                            
                            if (jsonObject.TryGetValue("choices", out var choicesToken) && 
                                choicesToken is JArray choicesArray && 
                                choicesArray.Count > 0)
                            {
                                var choice = choicesArray[0];
                                if (choice is JObject choiceObj && 
                                    choiceObj.TryGetValue("delta", out var deltaToken) && 
                                    deltaToken is JObject deltaObj &&
                                    deltaObj.TryGetValue("content", out var contentToken) && 
                                    contentToken.Type == JTokenType.String)
                                {
                                    var contentText = contentToken.ToString();
                                    if (!string.IsNullOrEmpty(contentText))
                                    {
                                        if (!firstContentTokenReceived)
                                        {
                                            firstTokenTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                                            firstContentTokenReceived = true;
                                        }
                                        
                                        responseContent.Append(contentText);
                                    }
                                }
                            }
                        }
                        catch (JsonException)
                        {
                            continue;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            
            if (responseContent.Length > 0)
            {
                tokenCount = TokenCalculator.CalculateTokens(responseContent.ToString());
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception(LocalizationManager.Get("api_call_exception", ex.Message));
        }
        finally
        {
            response?.Dispose();
            httpRequest?.Dispose();
        }
        
        var totalTimeSeconds = (DateTime.UtcNow - startTime).TotalSeconds;
        result.FirstTokenTime = firstTokenTime;
        result.TotalTokens = tokenCount;
        result.TokensPerSecond = totalTimeSeconds > 0 && tokenCount > 0 ? tokenCount / totalTimeSeconds : 0;
        
        return result;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

public static class TokenCalculator
{
    public static int CalculateTokens(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        
        try
        {
            var encoder = ModelToEncoder.For("gpt-4");
            return encoder.CountTokens(text);
        }
        catch (Exception ex)
        {
            Console.WriteLine(LocalizationManager.Get("token_calculation_warning", ex.Message));
            return EstimateTokensFallback(text);
        }
    }
    
    private static int EstimateTokensFallback(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        
        var charCount = text.Length;
        var wordCount = text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        
        var chineseCharCount = text.Count(c => c >= 0x4e00 && c <= 0x9fff);
        var chineseRatio = charCount > 0 ? (double)chineseCharCount / charCount : 0;
        
        int estimatedTokens;
        
        if (chineseRatio > 0.3)
        {
            var englishWordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Count(word => word.Any(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')));
            
            estimatedTokens = chineseCharCount + (int)(englishWordCount * 1.3);
        }
        else
        {
            double tokenRatio = wordCount <= 5 ? 1.8 : (wordCount <= 15 ? 1.5 : 1.3);
            estimatedTokens = (int)(wordCount * tokenRatio);
        }
        
        var punctuationCount = text.Count(c => char.IsPunctuation(c));
        estimatedTokens += (int)(punctuationCount * 0.8);
        
        return Math.Max(1, estimatedTokens);
    }
}

public class TestRunner
{
    private readonly Config _config;
    private readonly List<string> _questions;

    public TestRunner(Config config)
    {
        _config = config;
        _questions = GetTestQuestions();
    }

    public async Task<TestResult[]> RunTestsAsync()
    {
        using var apiClient = new ApiClient(_config);
        
        // 验证模型
        var modelValidationResult = await apiClient.ValidateModelAsync();
        if (!modelValidationResult.isValid)
        {
            Console.WriteLine(LocalizationManager.Get("model_validation_failed", modelValidationResult.errorMessage));
            return Array.Empty<TestResult>();
        }

        var tasks = new List<Task<TestResult>>();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_config.TimeoutSec));
        var random = new Random();
        var availableQuestions = new List<string>(_questions);

        for (int i = 0; i < _config.ConcurrentNum; i++)
        {
            if (availableQuestions.Count == 0)
            {
                Console.WriteLine(LocalizationManager.Get("no_completed_requests"));
                break;
            }

            var questionIndex = random.Next(availableQuestions.Count);
            var question = availableQuestions[questionIndex];
            availableQuestions.RemoveAt(questionIndex);

            tasks.Add(apiClient.SendRequestAsync(question, cts.Token));
        }

        try
        {
            return await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine(LocalizationManager.Get("timeout_collecting_results"));
            
            var completedResults = tasks
                .Where(t => t.IsCompletedSuccessfully)
                .Select(t => t.Result)
                .ToArray();
            
            if (!completedResults.Any())
            {
                Console.WriteLine(LocalizationManager.Get("no_completed_requests"));
            }
            
            return completedResults;
        }
    }

    private static List<string> GetTestQuestions()
    {
        return new List<string>
        {
            "Please write a comprehensive analysis of the economic implications of artificial intelligence on global labor markets, including both positive and negative effects, policy recommendations, and predictions for the next decade.",
            "Explain the detailed molecular mechanisms behind photosynthesis, including the light-dependent and light-independent reactions, the role of different pigments, and how environmental factors affect the process.",
            "Describe the complete process of protein synthesis from DNA transcription to protein folding, including the roles of mRNA, tRNA, ribosomes, and various enzymes involved in each step.",
            "Analyze the causes and consequences of climate change, discussing greenhouse gases, feedback loops, regional impacts, adaptation strategies, and international policy approaches.",
            "Provide a detailed explanation of quantum mechanics, including wave-particle duality, the uncertainty principle, quantum entanglement, and their applications in modern technology.",
            "Discuss the evolution of human civilization from the agricultural revolution to the digital age, examining key technological innovations, social changes, and their interconnected effects.",
            "Explain the structure and function of the human nervous system, including neuron types, signal transmission, brain regions, and the mechanisms underlying learning and memory.",
            "Analyze the principles of sustainable development, discussing environmental, economic, and social dimensions, along with practical implementation strategies for different sectors.",
            "Describe the fundamental forces of physics and their roles in the universe, from subatomic particles to cosmic structures, including recent discoveries and theoretical developments.",
            "Examine the relationship between technology and society, analyzing how digital transformation affects education, healthcare, governance, and social interactions.",
            "Explain the immune system's complex mechanisms for defending against pathogens, including innate and adaptive immunity, cellular and humoral responses, and autoimmune disorders.",
            "Discuss the principles of genetics and genomics, including DNA structure, gene expression, inheritance patterns, genetic engineering, and their ethical implications.",
            "Analyze the global water cycle and its relationship with climate systems, including precipitation patterns, ocean currents, groundwater dynamics, and human impacts.",
            "Explain the economic theories of market behavior, including supply and demand dynamics, market failures, behavioral economics, and the role of government intervention.",
            "Describe the process of stellar evolution from star formation to death, including nuclear fusion, stellar classification, supernovae, and the formation of neutron stars and black holes.",
            "Analyze the impact of urbanization on environmental sustainability, discussing urban planning, transportation systems, energy consumption, and green city initiatives.",
            "Explain the principles of cybersecurity, including threat vectors, encryption methods, network security protocols, and emerging challenges in the digital age.",
            "Discuss the role of biodiversity in ecosystem stability, examining species interactions, conservation strategies, habitat preservation, and the consequences of mass extinction.",
            "Analyze the development of renewable energy technologies, comparing solar, wind, hydroelectric, and other sources in terms of efficiency, cost, and environmental impact.",
            "Explain the complex relationships between nutrition, metabolism, and human health, including macronutrients, micronutrients, digestive processes, and disease prevention.",
            "Describe the geological processes that shape Earth's surface, including plate tectonics, volcanism, erosion, and the formation of different rock types and landforms.",
            "Analyze the psychological and neurological basis of human behavior, including cognitive processes, emotional regulation, social psychology, and mental health disorders.",
            "Explain the principles of chemical bonding and molecular interactions, including atomic structure, periodic trends, reaction mechanisms, and applications in materials science.",
            "Discuss the evolution of democratic systems, examining different forms of governance, electoral processes, civil rights, and challenges to democratic institutions.",
            "Analyze the role of international trade in global economic development, including trade theories, monetary systems, globalization effects, and regional economic integration.",
            "Explain the mechanisms of evolution and natural selection, including genetic variation, speciation, phylogenetics, and evidence from fossil records and molecular biology.",
            "Describe the structure and dynamics of the solar system, including planetary formation, orbital mechanics, asteroid belts, and the search for extraterrestrial life.",
            "Analyze the impact of mass media and communication technologies on society, examining information dissemination, cultural change, and the digital divide.",
            "Explain the principles of sustainable agriculture, including soil health, crop rotation, pest management, genetic modification, and food security challenges.",
            "Discuss the mathematical foundations of computer science, including algorithms, data structures, computational complexity, and their applications in artificial intelligence."
        };
    }
}

public static class ResultsFormatter
{
    public static void PrintResults(TestResult[] results)
    {
        Console.WriteLine("\n" + LocalizationManager.Get("test_results"));
        Console.WriteLine(new string('=', 100));
        
        foreach (var result in results)
        {
            var shortQuestion = result.Question.Length > 60 
                ? result.Question.Substring(0, 57) + "..." 
                : result.Question;
            
            Console.WriteLine(LocalizationManager.Get("question", shortQuestion));
            Console.WriteLine(LocalizationManager.Get("first_response_time", result.FirstTokenTime));
            Console.WriteLine(LocalizationManager.Get("output_speed", result.TokensPerSecond));
            Console.WriteLine(new string('-', 70));
        }
        
        var validResults = results.Where(r => r.TokensPerSecond > 0).ToArray();
        if (validResults.Any())
        {
            var avgFirstTokenTime = validResults.Average(r => r.FirstTokenTime);
            var avgTokensPerSecond = validResults.Average(r => r.TokensPerSecond);
            var totalTokens = validResults.Sum(r => r.TotalTokens);
            var totalTokensPerSecond = validResults.Sum(r => r.TokensPerSecond);
            
            Console.WriteLine("\n" + LocalizationManager.Get("statistics_summary"));
            Console.WriteLine(new string('=', 50));
            Console.WriteLine(LocalizationManager.Get("successful_requests", validResults.Length, results.Length));
            Console.WriteLine(LocalizationManager.Get("avg_first_response", avgFirstTokenTime));
            Console.WriteLine(LocalizationManager.Get("avg_output_speed", avgTokensPerSecond));
            Console.WriteLine(LocalizationManager.Get("total_output_tokens", totalTokens));
            Console.WriteLine(LocalizationManager.Get("total_token_output_speed", totalTokensPerSecond));
            Console.WriteLine(new string('=', 50));
        }
        else
        {
            Console.WriteLine("\n" + LocalizationManager.Get("no_successful_requests"));
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        // 读取配置文件
        var config = LoadConfig("config.yaml");
        
        // 初始化语言设置
        LocalizationManager.SetLanguage(config.Language);
        
        // 验证baseurl格式
        if (!IsValidUrl(config.BaseUrl))
        {
            Console.WriteLine(LocalizationManager.Get("invalid_api_url"));
            return;
        }
        
        // 确保baseurl以/v1或/v1/结尾
        if (!config.BaseUrl.EndsWith("/v1") && !config.BaseUrl.EndsWith("/v1/"))
        {
            config.BaseUrl = config.BaseUrl.TrimEnd('/') + "/v1";
        }
        
        if (config.ConcurrentNum < 1 || config.ConcurrentNum > 20)
        {
            Console.WriteLine(LocalizationManager.Get("concurrent_num_range"));
            return;
        }

        // 验证超时时间配置
        if (config.TimeoutSec < 1 || config.TimeoutSec > 120)
        {
            config.TimeoutSec = 10; // 使用默认值
        }
        
        // 输出测试信息
        var concurrentText = config.ConcurrentNum == 1 ? "" : LocalizationManager.Get("concurrent");
        var pluralSuffix = config.Language == "en" && config.ConcurrentNum > 1 ? "s" : "";
        Console.WriteLine(LocalizationManager.Get("starting_test", config.ConcurrentNum, concurrentText, pluralSuffix));
        Console.WriteLine(LocalizationManager.Get("api_url", config.BaseUrl));
        Console.WriteLine(LocalizationManager.Get("model", config.ModelName));
        Console.WriteLine(LocalizationManager.Get("timeout", config.TimeoutSec));
        Console.WriteLine(new string('-', 80));

        // 创建测试运行器
        var testRunner = new TestRunner(config);
        
        // 运行测试
        var results = await testRunner.RunTestsAsync();
        
        // 输出结果
        ResultsFormatter.PrintResults(results);
    }
    
    private static Config LoadConfig(string filePath)
    {
        var yamlContent = File.ReadAllText(filePath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<Config>(yamlContent);
    }
    
    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}