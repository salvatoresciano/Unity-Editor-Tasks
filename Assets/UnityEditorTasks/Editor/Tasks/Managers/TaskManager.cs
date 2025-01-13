using System.Collections.Generic;
using UnityEngine;

public class TaskManager : ScriptableObject
{
    public List<Task> Tasks = new List<Task>();

    public void AddTask(Task task)
    {
        Tasks.Add(task);
    }

    public void ClearTasks()
    {
        Tasks.Clear();
    }
}