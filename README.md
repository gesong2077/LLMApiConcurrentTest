# LLM API Concurrent Test / LLM API并发测试

A high-performance concurrent LLM API testing tool designed to evaluate streaming API endpoints under extreme concurrent conditions. This tool simulates real-world high-load scenarios to measure API response times, throughput, and concurrent performance limits for Large Language Models.

一个高性能的LLM API并发测试工具，专门用于在极端并发条件下评估流式API端点。该工具模拟真实世界的高负载场景，测量大型语言模型API的响应时间、吞吐量和并发性能极限。

## ✨ Features / 功能特点

### English
- **Concurrent Load Testing**: Test multiple simultaneous requests (1-20 concurrent connections)
- **Streaming API Support**: Optimized for streaming responses with real-time token counting
- **Accurate Token Metrics**: Uses official `tiktoken` library for precise token calculation
- **First Token Latency**: Measures time to receive the first content token
- **Multilingual Interface**: Supports both Chinese (cn) and English (en) output
- **Configurable Timeouts**: Intentionally short timeouts to ensure concurrent testing accuracy
- **Comprehensive Metrics**: Response time, token generation speed, success rates
- **Model Validation**: Automatic API endpoint and model validation with suggestions

### 中文
- **并发负载测试**: 支持多个同时请求测试 (1-20个并发连接)
- **流式API支持**: 针对流式响应优化，实时token计数
- **精确Token指标**: 使用官方`tiktoken`库进行精确token计算
- **首Token延迟**: 测量接收第一个内容token的时间
- **多语言界面**: 支持中文(cn)和英文(en)输出
- **可配置超时**: 故意设置短超时以确保并发测试准确性
- **全面指标**: 响应时间、token生成速度、成功率
- **模型验证**: 自动API端点和模型验证，提供建议

## 🚀 Quick Start / 快速开始

### Installation / 安装

```bash
git clone https://github.com/yourusername/LLMApiConcurrentTest.git
cd LLMApiConcurrentTest
dotnet restore
```

### Configuration / 配置

Edit `config.yaml` to configure your test parameters / 编辑 `config.yaml` 配置测试参数:

```yaml
# Number of concurrent requests (1-20)
concurrent_num: 3

# API Base URL
baseurl: "https://api.openai.com/v1"

# API Key
api_key: "your-api-key-here"

# Model Name
model_name: "gpt-4o-mini"

# Timeout in seconds (1-120s, default 10s)
# Important: Intentionally set short timeout to interrupt before AI completes response
# Purpose: Prevent some requests finishing early causing speed fluctuation in others, ensuring accurate concurrent test data
timeoutsec: 10

# Language Setting (cn/en)
language: "en"
```

#### Configuration Options

| Option | Description | Default | Valid Values |
|--------|-------------|---------|--------------|
| `concurrent_num` | Number of simultaneous requests | 3 | 1-20 |
| `baseurl` | API endpoint URL | - | Valid HTTP/HTTPS URL |
| `api_key` | Authentication key | "" | String |
| `model_name` | AI model identifier | - | Model name string |
| `timeoutsec` | Request timeout (intentionally short to ensure concurrent test accuracy) | 10 | 1-120 seconds |
| `language` | Interface language | "cn" | "cn" or "en" |

### 🏃 Usage

**Run the application:**
```bash
dotnet run
```

**Sample output:**
```
Starting test with 3 concurrent requests
API URL: https://api.openai.com/v1
Model: gpt-4o-mini
Timeout: 15 seconds
--------------------------------------------------------------------------------

🎯 Test Results:
====================================================================================================
Question: Analyze the causes and consequences of climate change...
First response time: 623.45 ms
Output speed: 12.34 Token/s
----------------------------------------------------------------------

📊 Statistics Summary:
==================================================
Successful requests: 3/3
Average first response time: 587.23 ms
Average output speed: 11.89 Token/s
Total output tokens: 456
Total token output speed: 35.67 Token/s
==================================================
```

### 🔍 Performance Metrics

- **First Response Time**: Time to receive the first content token
- **Output Speed**: Tokens generated per second for each request  
- **Total Token Output Speed**: Sum of all concurrent request speeds
- **Success Rate**: Percentage of completed requests

### ⏱️ Timeout Design Philosophy

The timeout parameter (`timeoutsec`) is **intentionally designed to be short** to ensure accurate concurrent testing:

**Why Short Timeout?**
- **Prevents Early Completion**: If some requests finish before others, it can cause speed fluctuations
- **Maintains Consistency**: All requests should experience similar processing time for fair comparison
- **Accurate Concurrency Testing**: Ensures all requests are competing for resources simultaneously

**Recommended Values:**
- **Development/Testing**: 5-15 seconds
- **Production Benchmarking**: 10-30 seconds
- **Stress Testing**: 1-5 seconds

**Note**: The goal is to interrupt requests before the AI completes its full response, allowing you to measure pure concurrent performance without bias from varying response lengths.

### 🏗️ Architecture

The application is built with a clean, modular architecture:

- **`ApiClient`**: Handles HTTP communication and streaming
- **`TestRunner`**: Manages test execution and coordination
- **`TokenCalculator`**: Accurate token counting using Tiktoken
- **`LocalizationManager`**: Multi-language support system
- **`ResultsFormatter`**: Output formatting and statistics

### 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### 🙏 Acknowledgments

- [OpenAI](https://openai.com/) for the API specification
- [Tiktoken](https://github.com/tryAGI/Tiktoken) for accurate token counting
- .NET community for excellent tooling and libraries

---

### 🇨🇳 中文版本

一个专业的API并发性能基准测试工具，用于测试流式API在高并发场景下的性能表现。

## 📋 系统要求

- .NET 8.0 或更高版本
- 支持的操作系统: Windows, macOS, Linux
- 网络连接 (用于API调用)

## ⚙️ 配置

编辑 `config.yaml` 文件来自定义你的测试设置:

```yaml
# 并发请求数量 (1-20)
concurrent_num: 3

# API基础URL
baseurl: "https://api.openai.com/v1"

# API密钥
api_key: "你的API密钥"

# 模型名称
model_name: "gpt-4o-mini"

# 超时时间(1-120秒，默认10秒)
# 重要：故意设置较短的超时时间，确保在AI回复完成前就中断请求
# 目的：防止部分请求先结束导致其他请求速度波动，保证并发测试数据准确性
timeoutsec: 10

# 语言设置 (cn/en)
language: "cn"
```

#### 配置选项

| 选项 | 描述 | 默认值 | 有效值 |
|------|------|--------|--------|
| `concurrent_num` | 同时请求数量 | 3 | 1-20 |
| `baseurl` | API 端点 URL | - | 有效的 HTTP/HTTPS URL |
| `api_key` | 认证密钥 | "" | 字符串 |
| `model_name` | AI 模型标识符 | - | 模型名称字符串 |
| `timeoutsec` | 请求超时时间（故意设置较短以确保并发测试准确性） | 10 | 1-120 秒 |
| `language` | 界面语言 | "cn" | "cn" 或 "en" |

### 🏃 使用方法

**运行应用程序:**
```bash
dotnet run
```

**示例输出:**
```
开始测试，使用 3 个并发请求
API地址: https://api.openai.com/v1
模型: gpt-4o-mini
超时时间: 15 秒
--------------------------------------------------------------------------------

🎯 测试结果:
====================================================================================================
问题: 分析气候变化的原因和后果...
首次响应时间: 623.45 ms
输出速度: 12.34 Token/s
----------------------------------------------------------------------

📊 统计摘要:
==================================================
成功请求数: 3/3
平均首次响应时间: 587.23 ms
平均输出速度: 11.89 Token/s
总计输出Token: 456
总计Token输出速度: 35.67 Token/s
==================================================
```

### 🔍 性能指标

- **首次响应时间**: 接收到第一个内容 token 的时间
- **输出速度**: 每个请求每秒生成的 token 数
- **总计Token输出速度**: 所有并发请求速度的总和
- **成功率**: 完成请求的百分比

### ⏱️ 超时设计理念

超时参数 (`timeoutsec`) 是 **故意设计为短** 以确保准确的并发测试：

**为什么设置短超时？**
- **防止提前完成**: 如果某些请求先于其他请求完成，可能会导致速度波动
- **保持一致性**: 所有请求应体验相似的处理时间，以确保公平比较
- **准确并发测试**: 确保所有请求同时竞争资源

**推荐值:**
- **开发/测试**: 5-15 秒
- **生产基准测试**: 10-30 秒
- **压力测试**: 1-5 秒

**注意**: 目标是中断请求，在 AI 完成完整响应之前，以便您可以测量纯并发性能，而不会受到响应长度变化的影响。

### 🏗️ 架构设计

应用程序采用清晰的模块化架构：

- **`ApiClient`**: 处理 HTTP 通信和流式传输
- **`TestRunner`**: 管理测试执行和协调
- **`TokenCalculator`**: 使用 Tiktoken 进行精确的 token 计算
- **`LocalizationManager`**: 多语言支持系统
- **`ResultsFormatter`**: 输出格式化和统计

### 🤝 贡献

1. Fork 此仓库
2. 创建功能分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'Add amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 开启 Pull Request

### 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

### 🙏 致谢

感谢以下开源项目的支持:
- [Tiktoken](https://github.com/tryAGI/Tiktoken) - Token计算
- [YamlDotNet](https://github.com/aaubry/YamlDotNet) - YAML配置解析
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) - JSON处理

## 📥 安装

```bash
git clone https://github.com/yourusername/LLMApiConcurrentTest.git
cd LLMApiConcurrentTest
dotnet restore
dotnet build
```

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🙏 致谢

感谢以下开源项目的支持:
- [Tiktoken](https://github.com/tryAGI/Tiktoken) - Token计算
- [YamlDotNet](https://github.com/aaubry/YamlDotNet) - YAML配置解析
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) - JSON处理

## ⭐ 项目统计

[![Star History Chart](https://api.star-history.com/svg?repos=yourusername/LLMApiConcurrentTest&type=Date)](https://star-history.com/#yourusername/LLMApiConcurrentTest&Date)