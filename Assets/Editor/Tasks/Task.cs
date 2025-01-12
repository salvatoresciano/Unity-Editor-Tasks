using System;

[Serializable]
public class Task
{
    public string Title;
    public string Notes;
    public TaskCategory Category;
    public TaskPriority Priority = TaskPriority.None;
    public DateTime CreationDate;
    public bool IsCompleted;
    public bool IsExpanded = false; // Default to collapsed state
}