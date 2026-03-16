OrchX - 萬能 AI 自動化助手 (Universal AI Automation Agent)
OrchX 是一個基於 .NET Framework 4.7.2 開發的高性能 C# AI Agent 系統。它不僅能與 Google Gemini API 進行對話，更核心的設計在於其「工具調度」與「多專家協作」能力，旨在打造一個能自我優化、具備環境感知能力的自動化工作站。

🌟 核心特色
1. 多專家協作模式 (Multi-Agent Ecosystem)
透過 consult_expert 工具，主 Agent 可以動態建立多個領域專家（如安全性專家、架構師、程式碼審查員）。

獨立記憶：每位專家擁有獨立的對話歷史與上下文。

多輪對話：支援與特定專家進行深度的往復討論。

2. 模組化工具箱 (Modular Toolset)
採用插件式架構，易於擴充：

檔案系統整合：

樹狀目錄檢視 (list_files)，支援深層探索。

智慧讀寫：支援大檔案自動摘要（使用 Fast Model）與特定行修改 (update_file_line)。

技能與規範：內建 read_skills/write_skill 功能，允許 Agent 自行建立與學習操作規範。

網路請求：完整的 http_get 與 http_post 支援，可串接外部 API。

模型調度：支援 smart (推理) 與 fast (速度) 雙模式切換。

3. 自動化與自癒能力
環境自適應：啟動時自動檢查 .env，若缺失則自動建立，並能連線 API 查詢可用模型清單。

Mock API 模式：在沒有 API Key 的情況下，可自動讀取 MockData/ 下的 JSON 回應，方便開發測試。

故障備份：發生嚴重錯誤或使用者中斷時，系統會自動將對話歷史備份至 JSON 檔案。

📁 專案結構
Plaintext
OrchX/
├── AIClient/           # Gemini API 通訊層，支援 Function Calling 封裝
├── Agents/             # Agent 核心邏輯
│   └── Modules/        # 功能模組 (File, Http, AIControl, MultiAgent)
├── Config/             # 系統指令 (System Instruction) 與配置管理
├── Tools/              # 底層工具 (JSON 處理、檔案 I/O、日誌紀錄)
├── UI/                 # 抽象化 UI 介面 (目前實作 ConsoleUI)
├── MockData/           # 離線測試用的模擬 API 回應
└── AI_Workspace/       # AI 的作業區，包含 .agent (規範與知識庫)
🛠 技術棧
Language: C# 7.3+

Framework: .NET Framework 4.7.2

Library:

Newtonsoft.Json: 高效 JSON 序列化

System.Net.Http: 異步網路請求

System.IO.Compression: 處理 .docx 等壓縮文件文字提取

🚀 快速上手
複製專案：

Bash
git clone https://github.com/your-repo/OrchX.git
初始化配置：
直接執行 OrchX.exe，程式會自動產生 .env 檔案。

設定 API Key：
開啟 .env，填入你的 Google AI Studio API Key：

程式碼片段
GEMINI_API_KEY=your_actual_key_here
GEMINI_MODEL=gemini-2.0-flash
開始互動：
再次執行程式，即可在 Console 中輸入指令。

⌨️ 內建指令
在對話輸入框中可以使用以下斜線指令：

/help : 顯示說明清單。

/new  : 清除目前對話紀錄，開啟新任務。

/save [path] : 儲存目前的對話紀錄。

/load [path] : 載入先前的對話紀錄。

/time : 開啟/關閉訊息的時間戳記。

/exit : 結束程式。

🛡️ 開發品質與驗證 (Senior Dev Validator)
本專案遵循資深開發規範，所有代碼異動皆建議經過以下驗證：

架構檢查：Namespace 必須與資料夾路徑對齊。

編譯驗證：確保執行 dotnet build 無誤。

日誌追蹤：所有 API 呼叫與工具執行紀錄均儲存於 logs/ 資料夾中。

Copyright © 2026 Antigravity Project

-----

OrchX - Universal AI Automation Agent
OrchX is a high-performance AI Agent system built on .NET Framework 4.7.2. It goes beyond simple chat interactions with Google Gemini API by implementing a sophisticated "Tool Orchestration" and "Multi-Expert Collaboration" framework. It is designed to be a self-optimizing, environment-aware automation workstation.

🌟 Key Features
1. Multi-Agent Ecosystem
Through the consult_expert tool, the main agent can dynamically spawn and consult specialized AI experts (e.g., Security Expert, Architect, Code Reviewer).

Independent Sessions: Each expert maintains its own isolated conversation history and context.

Multi-turn Reasoning: Supports deep, back-and-forth discussions with specific experts to solve complex tasks.

2. Modular Toolset
The system utilizes a pluggable architecture for easy expansion:

FileSystem Integration:

Tree View: Explore deep directory structures using list_files.

Smart I/O: Supports automatic summarization for large files (via Fast Model) and targeted line updates with update_file_line.

Skills & Rules: Includes read_skills and write_skill features, allowing the agent to create and learn its own operational procedures.

Networking: Full support for http_get and http_post to interface with external APIs.

Model Orchestration: Dynamic switching between Smart (Reasoning) and Fast (Speed) modes.

3. Automation & Self-Healing
Environment Adaptation: Automatically checks for .env files on startup; creates missing templates and fetches the latest available Gemini models from the API.

Mock API Mode: When an API Key is absent, the system reads pre-defined responses from the MockData/ directory for offline testing.

Failure Recovery: Automatically backs up conversation history to JSON files during critical errors or user interruptions.

📁 Project Structure
Plaintext
OrchX/
├── AIClient/           # Gemini API communication layer with Function Calling
├── Agents/             # Core Agent logic and BaseAgent implementation
│   └── Modules/        # Functional modules (File, Http, AIControl, MultiAgent)
├── Config/             # System Instructions and configuration management
├── Tools/              # Utilities (JSON, File I/O, Usage Logger)
├── UI/                 # Abstraction for UI interfaces (ConsoleUI)
├── MockData/           # Mock JSON responses for offline development
└── AI_Workspace/       # Dedicated workspace for AI operations (.agent rules/knowledge)
🛠 Tech Stack
Language: C# 7.3+

Framework: .NET Framework 4.7.2

Libraries:

Newtonsoft.Json: For high-performance JSON serialization.

System.Net.Http: For asynchronous network requests.

System.IO.Compression: For text extraction from compressed formats like .docx.

🚀 Getting Started
Clone the Repository:

Bash
git clone https://github.com/your-repo/OrchX.git
Initialize Configuration:
Run OrchX.exe once; the program will automatically generate a .env file.

Set API Key:
Open .env and enter your Google AI Studio API Key:

程式碼片段
GEMINI_API_KEY=your_actual_key_here
GEMINI_MODEL=gemini-2.0-flash
Start Interaction:
Launch the application again to start commanding the agent via the console.

⌨️ Built-in Commands
The following slash commands can be used in the input prompt:

/help : List all available commands.

/new  : Clear current history and start a new session.

/save [path] : Save current chat history to a file.

/load [path] : Load previous chat history.

/time : Toggle timestamps for user messages.

/exit : Close the program.

Copyright © 2026 Antigravity Project


