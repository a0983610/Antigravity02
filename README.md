OrchX: 終端機 AI 自動化代理系統
OrchX 是一個基於 C# (.NET 8.0) 開發的命令列 AI Agent 專案。系統設計的核心在於「工具調度 (Tool Orchestration)」與「多代理協作 (Multi-Agent Collaboration)」，透過定義明確的工具介面，讓 AI 模型能夠在受控的沙盒環境中執行檔案操作、終端機指令、HTTP 請求，並能動態生成子代理來處理複雜任務。

系統目前原生支援 Google Gemini API 與 Ollama (本地模型)。

核心功能
多代理協作機制 (Multi-Agent):
主 Agent 可透過 consult_expert 工具動態建立具有獨立上下文的特定領域專家 Agent。支援同步詢問與非同步 (背景) 執行，適合處理需要深度思考或多步驟的子任務。

多重 AI 後端與模式切換:

支援雲端 Gemini API 與本地 Ollama 部署。

實作 Smart (高推理) 與 Fast (高效能) 雙模型架構，Agent 可透過 switch_model_mode 自行決策切換，以平衡執行速度與 API 成本。

受限的模組化工具 (Modular Tools):

檔案系統 (FileModule): 支援目錄讀取、精準的單行/多行修改、內容搜尋。大型檔案讀取會自動調用 Fast 模型進行摘要處理。

終端機 (TerminalModule): 嚴格的白名單指令執行 (如 python, npm, git)。具備安全攔截機制：禁止重定向、管線符號，且所有指令送出前均會向使用者說明動機並要求 Y/N 授權。

網路請求 (HttpModule): 基礎的 HTTP GET / POST 操作。

沙盒環境隔離:
所有的檔案操作均透過 FileTools 進行路徑檢查，嚴格限制讀寫範圍於 AI_Workspace/ 目錄內，防止路徑穿越 (Path Traversal) 攻擊。

自動上下文壓縮:
當對話 Token 超出預設閾值（約 10 萬 Token）時，系統會自動使用 Fast 模型將前半段對話壓縮為結構化摘要，維持長對話的穩定性與效能。

離線開發與 Mock 機制:
內建 API 回應錄製 (/rmock) 功能。在沒有網路或 API Key 的環境下，系統可讀取 MockData/ 中的資料進行離線測試與重構。

專案架構
OrchX/
├── AIClient/      # AI 平台介接層 (GeminiClient, OllamaClient) 與請求/回應格式轉換
├── Agents/        # Agent 核心邏輯
│   ├── Base/      # 執行迴圈、歷史紀錄與 Token 管理 (BaseAgent)
│   ├── Modules/   # 工具模組實作 (File, Http, Terminal, AIControl, MultiAgent)
│   ├── ManagerAgent.cs # 整合所有模組的主代理
│   └── ExpertAgent.cs  # 具備特定系統指令的動態專家代理
├── Tools/         # 底層實用工具 (FileTools, HttpTools, TaskOrchestrator, Logger)
├── UI/            # 終端機介面實作，包含多行輸入、按鍵監聽與指令提示
├── Config/        # 系統設定與預設 System Prompt 管理
├── CommandManager.cs # 斜線 (/) 系統指令註冊與分派
└── AI_Workspace/  # 系統預設的檔案沙盒操作目錄 (執行時自動建立)

(註：.agent、.claude 及 CLAUDE.md 等目錄與檔案為 AI 輔助開發的歷史紀錄與設定檔，不屬於核心程式邏輯範圍。)

環境變數設定:
首次執行專案時，會在程式根目錄自動生成 .env 檔案。請依需求填寫：
# Gemini API 設定 (優先)
GEMINI_API_KEY=你的_API_KEY
GEMINI_SMART_MODEL=gemini-2.5-flash
GEMINI_FAST_MODEL=gemini-2.5-flash

# Ollama 本地端設定 (當 API KEY 為空時自動啟用)
OLLAMA_URL=http://localhost:11434
OLLAMA_MODEL=gemma4

系統指令表
在互動介面中，支援按鍵 Tab 提示與自動補全，輸入 / 可呼叫系統指令：
指令,說明
/help,顯示所有可用指令列表
/new,清除當前對話歷史，開啟全新對話上下文
/save [path],將目前的對話歷史匯出為 JSON 檔案 (預設 chat_history.json)
/load [path],從指定的 JSON 檔案載入對話歷史
/time,開關使用者訊息前的時間戳記 (預設關閉)
/rmock,開關 API 模擬資料錄製模式
/test,開關 Request/Response 原始 JSON 測試紀錄
/exit,結束程式

錯誤處理與日誌
執行日誌: 位於 logs/ 目錄，按日記錄 API 呼叫耗時、Token 用量與工具執行結果。

錯誤日誌: 位於 err/ 目錄，當 API 發生非預期錯誤 (如 HTTP 500) 時，會將完整的 Request 與 Response 寫入此處供除錯分析。

防呆備份: 程式遇到未處理的例外強制關閉時，會嘗試將當前對話紀錄備份至 system_error_backup.json。

