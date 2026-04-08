OrchX: 終端機 AI 自動化代理系統
OrchX 是一個基於 C# (.NET 8.0) 開發的命令列 AI Agent 專案。系統設計核心在於「工具調度 (Tool Orchestration)」與「多代理協作 (Multi-Agent Collaboration)」，讓 AI 模型能在受控的沙盒環境中執行檔案操作、終端指令、HTTP 請求，並能動態生成子代理處理複雜任務。

🌟 核心功能
1. 多代理協作機制 (Multi-Agent)
動態專家系統: 主 Agent 可透過 consult_expert 工具建立具獨立上下文的特定領域專家。

執行模式: 支援同步詢問與非同步（背景）執行，適合處理深度思考或多步驟子任務。

2. 多重 AI 後端與模式切換
雙引擎支援: 支援雲端 Google Gemini API 與本地 Ollama 部署。

Smart/Fast 架構: 實作高推理 (Smart) 與高效能 (Fast) 雙模型，Agent 可透過 switch_model_mode 自行決策切換，平衡速度與成本。

3. 受限的模組化工具 (Modular Tools)
檔案系統 (FileModule): 支援目錄讀取、精準修改、內容搜尋。大型檔案會自動調用 Fast 模型進行摘要。

終端機 (TerminalModule):

嚴格白名單制 (如 python, npm, git)。

安全攔截：禁止重定向、管線符號。

人工介入: 所有指令執行前須經使用者 Y/N 授權。

網路請求 (HttpModule): 基礎 HTTP GET / POST 操作。

4. 系統穩定性與安全性
沙盒隔離: 嚴格限制讀寫範圍於 AI_Workspace/ 目錄，防止路徑穿越攻擊。

自動上下文壓縮: 當對話超過閾值（約 10 萬 Token）時，自動使用 Fast 模型將前半段壓縮為結構化摘要。

離線開發 (Mock): 支援 API 回應錄製 (/rmock)，可在無網路環境下進行測試與重構。

📂 專案架構

OrchX/
├── AIClient/         # AI 平台介接層 (Gemini, Ollama) 與格式轉換
├── Agents/           # Agent 核心邏輯
│   ├── Base/         # 執行迴圈、歷史紀錄與 Token 管理 (BaseAgent)
│   ├── Modules/      # 工具模組實作 (File, Http, Terminal, AIControl, MultiAgent)
│   ├── ManagerAgent.cs # 整合所有模組的主代理
│   └── ExpertAgent.cs  # 具備特定系統指令的動態專家代理
├── Tools/            # 底層工具 (FileTools, HttpTools, TaskOrchestrator, Logger)
├── UI/               # 終端機介面 (多行輸入、按鍵監聽、提示)
├── Config/           # 系統設定與 System Prompt 管理
├── CommandManager.cs # 斜線 (/) 系統指令註冊與分派
└── AI_Workspace/     # 系統預設檔案沙盒操作目錄

⚙️ 環境設定
首次執行時，系統會自動生成 .env 檔案，請依需求填寫：

# Gemini API 設定 (優先使用)
GEMINI_API_KEY=你的_API_KEY
GEMINI_SMART_MODEL=gemini-2.5-flash
GEMINI_FAST_MODEL=gemini-2.5-flash

# Ollama 本地端設定 (當 API KEY 為空時自動啟用)

OLLAMA_URL=http://localhost:11434
OLLAMA_MODEL=gemma4

⌨️ 系統指令表
在互動介面輸入 / 即可呼叫指令，支援 Tab 鍵 自動補全：

指令,說明
/help,顯示所有可用指令列表
/new,清除當前歷史，開啟全新對話
/save [path],匯出對話歷史為 JSON (預設 chat_history.json)
/load [path],從指定 JSON 檔案載入對話歷史
/time,開關訊息前的時間戳記
/rmock,開關 API 模擬資料錄製模式
/test,開關 Request/Response 原始 JSON 測試紀錄
/exit,結束程式

🛠️ 錯誤處理與日誌

執行日誌 (logs/): 按日記錄 API 耗時、Token 用量與工具結果。

錯誤日誌 (err/): 記錄 API 非預期錯誤 (HTTP 500) 的完整 Request/Response。

防呆備份: 程式異常損毀時，會自動將對話備份至 system_error_backup.json。
