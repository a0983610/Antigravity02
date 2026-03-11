Antigravity02 — C# 萬能 AI 自動化代理系統
Antigravity02 是一個基於 .NET 8 開發的終端機 AI Agent 應用程式。本專案以 Google Gemini 為大腦，實作了精密的 Function Calling (工具呼叫) 循環，使 AI 具備操作檔案、網路通訊、自我進化以及調度「專家代理人」的能力，旨在打造一個可自我擴充的自動化工作站。

🚀 核心亮點
1. 模組化工具箱 (Agent Modules)
系統採用解耦設計，賦予 AI 多維度的操作能力：

檔案系統沙盒 (FileModule)：具備完整的 CRUD、樹狀結構瀏覽與內容檢索功能。所有操作嚴格限制在 AI_Workspace/ 內。

視覺整合：支援將圖片路徑直接交給 AI，系統會自動轉碼並注入對話歷史，讓 AI「看見」工作區內的圖檔。

網路存取 (HttpModule)：具備標準的 GET 與 POST 請求能力，可直接與外部 Web API 互動。

2. 多代理專家協作 (Multi-Agent System)
動態專家調度 (consult_expert)：主代理可隨時諮詢特定領域的專家（如：架構專家、資安專家），每位專家擁有獨立的 System Instruction 與對話記憶。

非同步任務編排：支援背景執行任務。主代理可將耗時任務指派給專家後繼續執行其他指令，稍後再透過 check_task_status 獲取結果。

3. 雙模型運作與自動進化
Smart & Fast 雙模式：支援同時設定「推理模型 (如 Gemini 2.0 Pro)」與「快速模型 (如 Gemini 2.0 Flash)」。AI 可根據任務複雜度呼叫 switch_model_mode 自行切換。

自動對話壓縮：當 Token 累積達到閾值（預設 80 萬）時，系統會自動啟動 Fast 模型對前半段對話進行「關鍵資訊萃取」與摘要，並寫入知識庫，確保長效記憶不遺失。

自我指令優化 (refine_my_behavior)：AI 能根據執行經驗，向使用者提議更新自己的系統指令。

4. 知識庫與技能系統
長效記憶 (Knowledge Base)：透過 write_note 將資訊持久化。系統會自動維護 00_INDEX.md 索引，並在每次對話啟動時主動讀取索引供 AI 檢索。

技能擴充 (Skills)：AI 可將複雜的標準作業程序 (SOP) 封裝為 Markdown 格式的「技能」，儲存於 .agent/skills/ 中重複利用。

🛠️ 開發與健壯性
Mock 模式與錄製：支援在無 API Key 環境下運作，透過 /rmock 指令可將真實 API 回應錄製為 Mock Data，便於離線測試與重現 Bug。

強化穩定性：已修復多項關鍵 Bug（如 BUG-001 至 BUG-017），包含資源洩漏、執行緒安全、路徑穿越防護及非同步死鎖預防。

📖 快速上手
1. 環境配置
首次執行後會自動產生 .env 檔案，請填入 API 金鑰：

Ini, TOML
GEMINI_API_KEY=你的金鑰
GEMINI_SMART_MODEL=gemini-2.0-flash (或 Pro 版本)
GEMINI_FAST_MODEL=gemini-2.0-flash
系統啟動時會自動查詢可用模型清單並寫入 .env 供參考。

2. 控制台指令
在對話框輸入 / 即可觸發自動補全功能：

/new：開啟全新對話，清除記憶。

/save [filename]：將當前對話歷史存檔。

/load [filename]：載入歷史對話。

/time：切換訊息時間戳記顯示。

/exit：安全結束程式。

🛡️ 安全性規範
沙盒機制：所有檔案 I/O 必須通過 IsPathAllowed 驗證，禁止任何 .. 或超出 AI_Workspace 的存取。

敏感資訊保護：嚴禁 API Key 與機敏路徑外洩至日誌中。

Last Updated: 2026-03-11

---

Antigravity02 — Universal AI Automation Agent System (C#)
Antigravity02 is a terminal-based AI Agent application built on .NET 8. It leverages Google Gemini as its core engine and implements a sophisticated Function Calling loop, enabling the AI to operate files, handle network communications, self-evolve, and orchestrate "expert agents" to create a self-extensible automated workstation.

🚀 Key Highlights
1. Modular Toolset (Agent Modules)
The system features a decoupled design, granting the AI multi-dimensional operational capabilities:

File System Sandbox (FileModule): Provides full CRUD operations, directory tree browsing, and content indexing. All operations are strictly restricted within the AI_Workspace/ directory.

Visual Integration: Supports providing image paths directly to the AI. The system automatically encodes and injects images into the conversation history, allowing the AI to "see" files within the workspace.

Network Access (HttpModule): Equipped with standard GET and POST request capabilities to interact directly with external Web APIs.

2. Multi-Agent Expert Orchestration
Dynamic Expert Dispatch (consult_expert): The main agent can consult specialized sub-agents (e.g., Architecture Expert, Security Expert) at any time. Each expert maintains its own System Instruction and conversation memory.

Asynchronous Task Orchestration: Supports background task execution. The main agent can assign time-consuming tasks to an expert and continue handling other user inputs, querying results later via check_task_status.

3. Dual-Model Operation & Self-Evolution
Smart & Fast Modes: Supports simultaneous configuration of a "Reasoning Model" (e.g., Gemini 2.0 Pro) and a "Fast Model" (e.g., Gemini 2.0 Flash). The AI can call switch_model_mode to switch based on task complexity.

Automatic History Compression: When tokens exceed a threshold (default 800k), the system uses the Fast model to extract key information and summarize the history, saving it to the knowledge base to prevent memory loss.

Behavior Refinement (refine_my_behavior): The AI can propose updates to its own system instructions based on execution experience to optimize future performance.

4. Knowledge Base & Skills
Long-term Memory (Knowledge Base): Persists information via write_note. The system automatically maintains a 00_INDEX.md index, which is proactively read at the start of each session for AI retrieval.

Skill Expansion (Skills): The AI can encapsulate complex Standard Operating Procedures (SOPs) into Markdown-formatted "Skills" stored in .agent/skills/ for reuse.

🛠️ Development & Robustness
Mock Mode & Recording: Operates without an API key by reading JSON mock responses from the MockData/ directory. The /rmock command enables recording real API responses as Mock Data for offline testing.

Hardened Stability: Fixed critical bugs (BUG-001 to BUG-017) regarding resource leaks, thread safety, path traversal protection, and async deadlock prevention.

📖 Getting Started
1. Configuration
A .env file is generated on first run. Please fill in your API key:

Ini, TOML
GEMINI_API_KEY=your_key_here
GEMINI_SMART_MODEL=gemini-2.0-flash (or Pro version)
GEMINI_FAST_MODEL=gemini-2.0-flash
The system automatically queries available models and updates .env for reference upon startup.

2. Console Commands
Type / in the prompt to trigger autocomplete:

/new: Clear memory and start a new conversation.

/save [filename]: Export current conversation history.

/load [filename]: Import previous conversation history.

/time: Toggle message timestamp headers.

/exit: Safely exit the application.

🛡️ Security & Boundaries
Sandbox Mechanism: All file I/O must pass IsPathAllowed validation, blocking any .. or access outside AI_Workspace.

Data Protection: Sensitive information like API keys and system paths are strictly protected from leaking into logs.

Last Updated: 2026-03-11
