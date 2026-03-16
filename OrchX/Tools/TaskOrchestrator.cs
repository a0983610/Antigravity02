using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OrchX.Tools
{
    public enum TaskStatus
    {
        Pending,
        Running,
        Completed,
        Failed
    }

    public class TaskItem
    {
        public string TaskId { get; set; } = Guid.NewGuid().ToString("N");
        public string Assignee { get; set; } = string.Empty;
        public string Request { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public TaskStatus Status { get; set; } = TaskStatus.Pending;
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public bool IsDelivered { get; set; } = false;
    }

    public static class TaskOrchestrator
    {
        private static readonly ConcurrentDictionary<string, TaskItem> _tasks = new ConcurrentDictionary<string, TaskItem>();

        public static TaskItem AddTask(string expert, string question)
        {
            var task = new TaskItem
            {
                TaskId = Guid.NewGuid().ToString("N"),
                Assignee = expert,
                Request = question,
                Result = string.Empty,
                Status = TaskStatus.Pending,
                CreatedTime = DateTime.Now
            };

            _tasks.TryAdd(task.TaskId, task);
            return task;
        }

        public static void UpdateTask(string id, TaskStatus status, string result)
        {
            if (_tasks.TryGetValue(id, out var task))
            {
                task.Status = status;
                if (result != null)
                {
                    task.Result = result;
                }
            }
        }

        public static TaskItem GetTask(string id)
        {
            _tasks.TryGetValue(id, out var task);
            return task;
        }

        public static IEnumerable<TaskItem> GetActiveTasks()
        {
            return _tasks.Values.Where(t => t.Status == TaskStatus.Pending || t.Status == TaskStatus.Running);
        }

        /// <summary>
        /// 取得已完成但尚未交付給主 Agent 的任務結果
        /// </summary>
        public static IEnumerable<TaskItem> GetCompletedUndeliveredTasks()
        {
            return _tasks.Values.Where(t => (t.Status == TaskStatus.Completed || t.Status == TaskStatus.Failed) && !t.IsDelivered);
        }

        /// <summary>
        /// 將任務標記為已交付，避免重複顯示
        /// </summary>
        public static void MarkAsDelivered(string id)
        {
            if (_tasks.TryGetValue(id, out var task))
            {
                task.IsDelivered = true;
            }
        }
    }
}
