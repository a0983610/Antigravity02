# 🌌 OrchX: Universal AI Automation Agent

OrchX 是一個基於 .NET 8.0 開發的高效能 C# AI Agent 系統。它不僅能與 Google Gemini API 進行對話，更核心的設計在於其「工具調度 (Tool Orchestration)」與「多專家協作 (Multi-Agent Collaboration)」能力，打造一個能自我優化、具備環境感知能力的自動化工作站。

## ✨ 核心特色

### 1. 多專家協作生態 (Multi-Agent Ecosystem)
透過 `consult_expert` 工具，主 Agent 可動態建立多位領域專家（如安全性專家、架構師、程式碼審查員）：
* **獨立記憶**：每位專家擁有獨立的對話歷史與上下文，互不干擾。
* **深層對話**：支援同步或非同步任務指派 (`is_async`)，並能進行多輪深度的往復討論，背景完成後可透過 `read_task_result` 讀取結果。

### 2. 強大的模組化工具箱 (Modular Toolset)
採用插件式架構，易於擴充：
* **智慧檔案系統**：支援樹狀目錄檢視、大檔案自動摘要（Fast Model 預處理）及精準的單行修改 (`update_file_line`)。所有操作嚴格限制於 `AI_Workspace/` 沙盒內。
* **環境與網路**：整合完整的 HTTP GET/POST 請求 (`HttpModule`)，並具備終端指令執行能力（受控的白名單與防護模式 `TerminalModule`）。
* **技能學習**：AI 可自行建立與更新 `.agent/skills/` 下的 SOP 規範，實現能力的持續增長；並透過 `write_note` 全自動維護長期知識庫索引 (`00_INDEX.md`)。

### 3. 自動化與自癒能力
* **雙模型動態切換**：內建 Smart (聰明) 與 Fast (快速) 雙模型機制，AI 可視任務複雜度自行切換。
* **對話歷史壓縮**：當對話超過 Token 閾值（預設 80 萬 Token）時，自動觸發摘要壓縮以節省上下文空間。
* **Mock API 模式**：在無 API Key 環境下可自動回放 `MockData/` 內容，或利用 `/rmock` 錄製真實 API 請求，方便離線開發測試。
* **異常恢復**：發生嚴重錯誤或意外中斷時，系統會自動備份對話紀錄至 JSON 檔案。

## 📁 專案結構

```text
OrchX/
├── AIClient/      # Gemini API 客戶端，處理 Function Calling 與 HTTP 429 退避
├── Agents/        # Agent 核心邏輯與基底類別
│   └── Modules/   # 功能模組 (File, Http, AIControl, MultiAgent, Terminal)
├── AI_Workspace/  # AI 的沙盒作業區，包含 .agent 規則、技能與知識庫
├── Config/        # 系統指令 (SystemInstruction) 與代理設定檔
├── Tools/         # 底層工具 (JSON, File I/O, 日誌、非同步任務編排)
└── UI/            # 抽象化 UI 介面與命令列自動完成輸入輔助
```

🛠️ 技術棧
語言：C# 12.0+

框架：.NET 8.0 (LTS)

核心 AI：Google Gemini API (預設支援 gemini-2.5-flash)

第三方庫：Newtonsoft.Json (高效能 JSON 序列化)、System.Net.Http、System.IO.Compression

🚀 快速上手
環境準備：確保已安裝 .NET 8.0 SDK。

配置金鑰：

初次執行程式將自動於根目錄產生 .env 檔案。

在 .env 中填入您的 GEMINI_API_KEY。

建置並執行：
dotnet build -c Release
dotnet run
(若無填寫 API Key，系統將自動進入 Mock API 離線測試模式)

⌨️ 內建指令
在對話視窗中，您可以使用以下斜線指令控制系統：
指令,描述
/help,顯示所有可用指令說明。
/new,清除對話紀錄，開啟全新任務。
/save [path],儲存目前對話歷史至指定路徑 (預設為 chat_history.json)。
/load [path],載入先前的對話歷史檔案。
/time,開啟或關閉使用者訊息的時間戳記。
/rmock,切換是否錄製真實 API 回應為模擬資料。
/exit,安全結束程式。

🛡️ 開發品質與驗證 (Senior Dev Validator)
本專案遵循資深開發驗證流程，確保程式碼高品質，所有歷史重大問題 (如記憶體洩漏、路徑穿越漏洞) 均已修復並記錄於 BUGS.md：

🧠 思考與設計：所有異動需對齊 Clean Architecture 職責分離原則。

🛠️ 執行狀態：嚴格執行 dotnet build 驗證與依賴檢查。

🧪 測試與驗證：支援運行時驗證與自動化日誌追蹤 (logs/ Token 用量與 err/ 詳細 API 錯誤追蹤)。

Copyright © 2026 Antigravity Project

---

# 🌌 OrchX: Universal AI Automation Agent

OrchX is a high-performance C# AI Agent system built on .NET 8.0. More than just a chatbot interface for the Google Gemini API, its core design focuses on **Tool Orchestration** and **Multi-Agent Collaboration**, creating a self-optimizing, environment-aware automation workstation.

## ✨ Key Features

### 1. Multi-Agent Ecosystem
Through the `consult_expert` tool, the main Agent can dynamically spawn specialized experts (e.g., Security Expert, Architect, Code Reviewer):
* **Independent Memory**: Each expert maintains its own isolated conversation history and context.
* **Deep Reasoning**: Supports both synchronous and asynchronous (`is_async`) task assignments for rigorous back-and-forth discussions. Background tasks can be retrieved later using `read_task_result`.

### 2. Powerful Modular Toolset
Built with a pluggable architecture for seamless expansion:
* **Smart File System**: Features tree-view directory exploration, automatic large-file summarization (via Fast Model), and precise line-by-line updates (`update_file_line`). All operations are safely sandboxed within the `AI_Workspace/` directory.
* **Environment & Network**: Integrates full HTTP GET/POST requests (`HttpModule`) and terminal command execution via a strictly controlled whitelist mode (`TerminalModule`).
* **Skill Acquisition**: The AI can independently create and update SOPs under `.agent/skills/`, allowing for continuous capability growth, and automatically maintain a long-term knowledge index (`00_INDEX.md`) using `write_note`.

### 3. Automation & Self-Healing
* **Dual-Model Switching**: Dynamically switches between "Smart" and "Fast" models at runtime based on task complexity.
* **History Compression**: Automatically triggers summary compression when the conversation exceeds token thresholds (default 800k) to save context space.
* **Mock API Mode**: Automatically replays mock data from `MockData/` when no API Key is available, or records real API responses via `/rmock` to facilitate offline development.
* **Failure Recovery**: Automatically backs up chat history to JSON files during critical errors or manual interruptions.

## 📁 Project Structure

```text
OrchX/
├── AIClient/      # Gemini API client, handles Function Calling and HTTP 429 backoff
├── Agents/        # Core Agent logic and base classes
│   └── Modules/   # Functional modules (File, Http, AIControl, MultiAgent, Terminal)
├── AI_Workspace/  # AI sandbox area, containing .agent rules, skills, and knowledge base
├── Config/        # System instructions and agent configurations
├── Tools/         # Low-level utilities (JSON, File I/O, Logging, Task Orchestrator)
└── UI/            # Abstraction for UI interfaces and auto-complete input helpers
```

🛠️ Tech Stack
Language: C# 12.0+

Framework: .NET 8.0 (Updated to the latest LTS version)

Core AI: Google Gemini API (Defaults to gemini-2.5-flash)

Third-party Libraries: Newtonsoft.Json (High-performance JSON serialization), System.Net.Http, System.IO.Compression

🚀 Getting Started
Prerequisites: Ensure the .NET 8.0 SDK is installed.

Configuration:

Run the program for the first time to automatically generate a .env file in the root directory.

Fill in your GEMINI_API_KEY in the .env file.

Build & Run:
dotnet build -c Release
dotnet run

(If no API Key is provided, the system will seamlessly fall back to offline Mock API Mode)

⌨️ Built-in Commands
Use the following slash commands within the dialogue window to control the system:

Command,Description
/help,Lists all available command descriptions.
/new,Clears chat history and starts a fresh session.
/save [path],Saves current chat history to a specified path (default: chat_history.json).
/load [path],Loads a previous chat history file.
/time,Toggles timestamps for user messages.
/rmock,Toggles recording of real API responses as mock data.
/exit,Safely closes the program.

🛡️ Senior Dev Validator & Quality
This project follows professional senior development validation processes to ensure code quality. All major historical issues (e.g., resource leaks, path traversal vulnerabilities) have been patched and documented in BUGS.md:

🧠 Design & Thinking: All changes align with Clean Architecture principles and strict separation of concerns.

🛠️ Compilation Status: Verified through strict dotnet build execution.

🧪 Verification: Supports comprehensive runtime validation and automated log tracking (Token usage in logs/ and detailed API error dumps in err/).

Copyright © 2026 Antigravity Project






















