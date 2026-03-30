# 🌌 OrchX: 萬能 AI 自動化代理系統

OrchX 是一個基於 C# (.NET 8.0) 開發的終端機 AI Agent 個人實驗專案。本系統整合了 Google Gemini API，核心專注於「工具調度 (Tool Orchestration)」與「多專家協作 (Multi-Agent Collaboration)」，旨在打造一個具備沙盒環境感知、動態工具呼叫與自我記憶管理能力的自動化工作流程。

## ✨ 核心特色

* **多專家協作 (Multi-Agent Ecosystem):** 透過 `consult_expert` 工具，主 Agent 可以動態建立不同領域的專家（如架構師、程式碼審查員），支援獨立記憶與非同步背景任務執行 (`is_async`)，達成深度的多輪邏輯推演。
* **雙模型動態切換 (Smart/Fast Models):** 系統內建 Smart (預設用於複雜推理) 與 Fast (預設用於快速摘要) 雙模型機制。AI 可視任務複雜度自動呼叫 `switch_model_mode` 切換思考模式，兼顧效能與 API 成本。
* **模組化工具箱 (Modular Toolset):**
    * **檔案系統 (FileModule):** 支援目錄樹檢視、大檔案自動摘要（透過 Fast Model 預處理）與精準的單行代碼修改 (`update_file_line`)。
    * **終端機 (TerminalModule):** 支援執行白名單指令 (如 `python`, `npm`, `git`)。所有外部指令執行前皆會詳細列出目的，並需經使用者互動授權 (Y/N)，避免失控破壞。
    * **網路通訊 (HttpModule):** 內建標準的 HTTP GET / POST 請求能力。
* **自我進化與記憶管理 (Self-Evolution):** AI 能自行將標準化工作流程寫入 `.agent/skills/` 形成 SOP，並透過自動維護 `00_INDEX.md` 來管理 `.agent/knowledge/` 長期知識庫。
* **自動歷史壓縮:** 當對話超過 Token 閾值（預設 10 萬 Token）時，系統會自動將前半段對話壓縮為結構化摘要，防止上下文溢出並保持高執行效能。
* **Mock API 離線模式:** 內建資料錄製 (`/rmock`) 與回放功能，允許在無 API Key 的環境下進行離線開發與重構測試。

## 📁 專案架構

* `AIClient/`: 封裝 Gemini API 通訊底層，處理 Function Calling 解析與 429 Rate Limit 自動退避重試機制。
* `Agents/`: 包含萬能主代理 (`ManagerAgent`) 與特定領域專家 (`ExpertAgent`) 的執行循環與狀態管理。
* `Agents/Modules/`: 工具模組的實作介面 (File, Http, Terminal, AIControl, MultiAgent)，方便未來快速擴充新功能。
* `AI_Workspace/`: AI 操作的實體沙盒隔離區。系統所有的技能、知識筆記與系統行為微調設定皆落在此目錄。
* `Tools/`: 基礎設施工具，包含安全檔案 I/O、HTTP 封裝、非同步任務編排與錯誤 Log 記錄。

## 🚀 快速上手

**環境需求:** 請確認系統已安裝 `.NET 8.0 SDK`。

1.  **建置專案:**
    ```bash
    dotnet build -c Release
    ```
2.  **設定環境變數 (.env):**
    首次執行 `dotnet run` 時會在根目錄自動產生 `.env` 檔案。請填寫您的 API 金鑰：
    ```env
    GEMINI_API_KEY=你的_API_KEY
    GEMINI_SMART_MODEL=gemini-2.5-flash
    GEMINI_FAST_MODEL=gemini-2.5-flash
    ```
3.  **啟動互動模式:**
    ```bash
    dotnet run
    ```
    *(備註：若未配置 API Key，系統啟動時會自動降級至 Mock API 模式，讀取 `MockData/` 內的預設回應資料以供測試。)*

## ⌨️ 系統指令

在終端機介面中，支援自動補全 (Tab) 與下列斜線系統指令：

| 指令 | 說明 |
| :--- | :--- |
| `/help` | 顯示所有可用指令列表 |
| `/new` | 清除當前對話歷史，開啟全新上下文 |
| `/save [path]` | 將目前的對話歷史備份至 JSON 檔案 |
| `/load [path]` | 從指定的 JSON 檔案載入對話歷史 |
| `/time` | 開啟/關閉使用者訊息前的系統時間戳記 |
| `/rmock` | 開關真實 API 回應錄製功能 (存放至 MockData) |
| `/exit` | 安全結束程式 |

## 🛡️ 安全性與防護限制

本系統在設計上融入了嚴格的邊界檢查：
* **路徑穿越防護 (Path Traversal):** 所有的 `FileModule` 操作皆經過絕對路徑驗證，確保 AI 永遠無法存取或修改 `AI_Workspace/` 以外的實體系統檔案。
* **受限的執行環境:** `TerminalModule` 內建正則與前綴檢查，封鎖了 `>`、`|`、`&` 等串接符號及高風險指令 (如未帶 `--ignore-scripts` 的 `npm install`)。
* **例外自動備份:** 發生系統異常或手動強制中斷 (Ctrl+C) 時，對話狀態會自動 Dump 出來，避免重要思考進度遺失。

---

# 🌌 OrchX: Universal AI Automation Agent

OrchX is a terminal-based AI Agent personal experimental project built with C# (.NET 8.0). Integrating the Google Gemini API, its core focuses on **Tool Orchestration** and **Multi-Agent Collaboration**, aiming to create an automated workflow with sandbox environment awareness, dynamic tool calling, and self-memory management.

## ✨ Key Features

* **Multi-Agent Ecosystem:** Through the `consult_expert` tool, the main Agent can dynamically spawn specialized experts (e.g., Architect, Code Reviewer). It supports independent memory and asynchronous background task execution (`is_async`) for deep, multi-turn reasoning.
* **Dual-Model Dynamic Switching:** Built-in support for "Smart" (for complex reasoning) and "Fast" (for quick summaries) models. The AI can automatically call `switch_model_mode` to toggle its thinking mode based on task complexity, balancing performance and API costs.
* **Modular Toolset:**
    * **File System (FileModule):** Supports directory tree viewing, automatic large-file summarization (via the Fast Model), and precise single-line code updates (`update_file_line`).
    * **Terminal (TerminalModule):** Executes whitelisted commands (e.g., `python`, `npm`, `git`). All external commands are presented with their purpose and require explicit user authorization (Y/N) before execution to prevent unintended actions.
    * **Network (HttpModule):** Built-in standard HTTP GET / POST request capabilities.
* **Self-Evolution & Memory Management:** The AI can independently write standardized workflows into `.agent/skills/` as SOPs and automatically maintain a long-term knowledge base index (`00_INDEX.md`) under `.agent/knowledge/`.
* **Auto History Compression:** When the conversation exceeds the token threshold (default 100k tokens), the system automatically compresses the earlier half of the chat into a structured summary to prevent context overflow and maintain high performance.
* **Mock API Offline Mode:** Features built-in data recording (`/rmock`) and playback, allowing for offline development and refactoring tests without an API Key.

## 📁 Project Structure

* `AIClient/`: Encapsulates the Gemini API communication layer, handling Function Calling parsing and 429 Rate Limit auto-backoff retries.
* `Agents/`: Contains the execution loops and state management for the universal `ManagerAgent` and specific `ExpertAgent`s.
* `Agents/Modules/`: Implementation interfaces for tool modules (File, Http, Terminal, AIControl, MultiAgent) for easy future expansion.
* `AI_Workspace/`: The physical sandbox isolation area for AI operations. All skills, knowledge notes, and system behavior tweaks reside here.
* `Tools/`: Infrastructure utilities including safe file I/O, HTTP wrappers, async task orchestration, and error logging.

## 🚀 Getting Started

**Prerequisites:** Ensure the `.NET 8.0 SDK` is installed.

1.  **Build the Project:**
    ```bash
    dotnet build -c Release
    ```
2.  **Configure Environment Variables (.env):**
    Running `dotnet run` for the first time will automatically generate a `.env` file in the root directory. Please fill in your API key:
    ```env
    GEMINI_API_KEY=your_api_key_here
    GEMINI_SMART_MODEL=gemini-2.5-flash
    GEMINI_FAST_MODEL=gemini-2.5-flash
    ```
3.  **Start Interactive Mode:**
    ```bash
    dotnet run
    ```
    *(Note: If no API Key is configured, the system will automatically fall back to Mock API mode on startup, reading default responses from `MockData/` for testing purposes.)*

## ⌨️ System Commands

The terminal interface supports auto-completion (Tab) and the following slash system commands:

| Command | Description |
| :--- | :--- |
| `/help` | Lists all available commands |
| `/new` | Clears the current chat history, starting a fresh context |
| `/save [path]` | Backs up the current chat history to a JSON file |
| `/load [path]` | Loads chat history from a specified JSON file |
| `/time` | Toggles the system timestamp before user messages |
| `/rmock` | Toggles the recording of real API responses (saved to MockData) |
| `/exit` | Safely closes the program |

## 🛡️ Security & Sandboxing

This system is designed with strict boundary checks:
* **Path Traversal Protection:** All `FileModule` operations undergo absolute path validation, ensuring the AI can never access or modify physical system files outside of `AI_Workspace/`.
* **Restricted Execution Environment:** The `TerminalModule` features built-in regex and prefix checks, blocking chaining operators like `>`, `|`, `&` and high-risk commands (e.g., `npm install` without `--ignore-scripts`).
* **Exception Auto-Backup:** In the event of a system crash or manual interruption (Ctrl+C), the conversation state is automatically dumped to prevent the loss of important reasoning progress.
