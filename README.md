Antigravity02 - 萬能 AI 自動化助手
Antigravity02 是一個基於 .NET Framework 4.7.2 開發的 C# AI Agent 系統。它不僅能與 Google Gemini API 進行對話，更具備強大的「工具調度」與「多專家協作」能力。

🌟 核心特色
多專家協作模式 (Multi-Agent)：支援 consult_expert 功能。主 Agent 可以根據需求建立不同的領域專家（如安全性專家、架構專家），且每位專家擁有獨立的對話歷史與記憶。

模組化工具箱 (Modular Tools)：

檔案操作：支援樹狀清單檢視、讀寫檔案、更新特定行內容，並具備大檔案自動摘要功能。

網路請求：支援發送 HTTP GET 與 POST 請求。

模型切換：可動態切換 Smart (推理) 與 Fast (快速) 模型模式。

環境配置自動化：程式啟動時會自動檢查 .env 檔案，若缺失會自動建立；若未設定模型，會透過 API 自動查詢目前帳戶可用的 Gemini 模型清單並更新至配置中。

強健的錯誤處理：具備詳細的 UsageLogger 與 UsageLogger.LogApiError 機制，並在系統發生嚴重錯誤時自動備份對話紀錄。

🛠 系統需求
.NET Framework 4.7.2 或更高版本

Google Gemini API Key

🚀 快速上手
複製專案至本地。

執行程式，程式將自動產生 .env 檔案。

在 .env 中填入你的 GEMINI_API_KEY。

再次執行 Antigravity02.exe，即可開始互動。

📁 專案結構
AIClient/: 封裝 Gemini API 通訊邏輯。

Agents/: 核心 Agent 邏輯，包含 UniversalAgent 與 BaseAgent。

Agents/Modules/: 各種功能模組（檔案、HTTP、多專家系統）。

Tools/: 底層工具類別（檔案讀寫、日誌紀錄）。


Antigravity02 - Universal AI Automation Agent
Antigravity02 is a versatile C# AI Agent system built on .NET Framework 4.7.2. It leverages Google Gemini's advanced capabilities to orchestrate tools and specialized experts for complex automation tasks.

🌟 Key Features
Multi-Agent Ecosystem: Through the consult_expert tool, the main agent can spawn and consult specialized AI experts. Each expert maintains its own independent session history for coherent multi-turn reasoning.

Extensible Module Architecture:

FileSystem Integration: List directory structures, read/write/update files, and automatically summarize large documents using a faster model.

HTTP Networking: Perform GET and POST requests to interact with external APIs.

Dual-Model Strategy: Support for separate "Smart" and "Fast" model configurations to optimize performance and cost.

Smart Configuration: On startup, the system automatically detects environment settings. It can fetch the latest list of available Gemini models directly from the API and populate your .env file automatically.

Reliability: Includes a robust error logging system and automatic chat history backup in case of unexpected failures.

🛠 Prerequisites
.NET Framework 4.7.2+

A valid Google Gemini API Key

🚀 Getting Started
Clone the repository.

Run the application; it will automatically generate a .env template.

Add your GEMINI_API_KEY to the .env file.

Launch Antigravity02.exe and start commanding your agent.

📁 Project Overview
AIClient/: Gemini API client implementation and request/response handling.

Agents/: Core agent logic and specialized module registration.

Agents/Modules/: Functional tools including File I/O, HTTP requests, and Multi-Agent management.

Tools/: Helper classes for logging and low-level system operations.
