using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TaskManagerEditor : EditorWindow
{
    private TaskManager taskManager;
    private Vector2 scrollPosition;
    private string searchQuery = ""; // For task filtering

    [MenuItem("Tools/Task Manager")]
    public static void ShowWindow()
    {
        GetWindow<TaskManagerEditor>("Task Manager");
    }

    private void OnEnable()
    {
        LoadTasks();
    }

    private void OnDisable()
    {
        SaveTasks();
    }

    private void OnGUI()
    {
        if (taskManager == null)
        {
            taskManager = CreateInstance<TaskManager>();
        }

        EditorGUILayout.LabelField("Task Manager", EditorStyles.boldLabel);

        // Search bar for task filtering
        searchQuery = EditorGUILayout.TextField("Search", searchQuery);

        // Display task statistics
        ShowTaskStatistics();

        int index = 0;
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (var task in taskManager.Tasks.Where(t => t.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)))
        {
            DrawTask(task, index);
            index++;
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add New Task"))
        {
            taskManager.AddTask(new Task { Title = "New Task", CreationDate = DateTime.Now, Priority = TaskPriority.Low });
        }

        if (GUILayout.Button("Clear All Tasks"))
        {
            if (EditorUtility.DisplayDialog("Clear All Tasks", "Are you sure you want to clear all tasks?", "Yes", "No"))
            {
                taskManager.ClearTasks();
            }
        }

        if (GUILayout.Button("Export Tasks to JSON"))
        {
            ExportToJson();
        }

        if (GUILayout.Button("Import Tasks from JSON"))
        {
            ImportFromJson();
        }
    }

    private void DrawTask(Task task, int index)
    {
        // Begin a vertical box for the task
        EditorGUILayout.BeginVertical("box");

        // Determine the status color based on the task's state
        Color statusColor;
        if (task.IsCompleted)
        {
            statusColor = Color.green; // Completed tasks are green
        }
        else if (task.CreationDate < DateTime.Now && !task.IsCompleted)
        {
            statusColor = Color.red; // Overdue tasks are red
        }
        else
        {
            statusColor = new Color(1f, 0.647f, 0f); // In-progress tasks are orange
        }

        // Begin a horizontal row for the task details
        EditorGUILayout.BeginHorizontal();

        // Display the task title
        task.Title = EditorGUILayout.TextField("Task", task.Title);

        // End the horizontal row
        EditorGUILayout.EndHorizontal();

        // Create a foldout for each task, the default state is collapsed (false)
        task.IsExpanded = EditorGUILayout.Foldout(task.IsExpanded, "Task Details", true);

        if (task.IsExpanded)
        {

            // If the task is expanded, show the task details

            // Begin a horizontal row for the checkbox
            EditorGUILayout.BeginHorizontal();
            task.IsCompleted = EditorGUILayout.Toggle(task.IsCompleted, GUILayout.Width(20)); // Checkbox for completion
            EditorGUILayout.LabelField("Status: " + (task.IsCompleted ? "Completed" : "In Progress"));
            EditorGUILayout.EndHorizontal();

            // Priority dropdown
            task.Priority = (TaskPriority)EditorGUILayout.EnumPopup("Priority", task.Priority);

            // Category dropdown
            task.Category = (TaskCategory)EditorGUILayout.EnumPopup("Category", task.Category);

            // Due date field
            task.CreationDate = DateTimeEditorExtension.DateField("Due Date", task.CreationDate);

            // Notes for the task
            task.Notes = EditorGUILayout.TextArea(task.Notes, GUILayout.Height(50));

            // Display due date reminder if it's near
            if (task.CreationDate <= DateTime.Now.AddDays(1) && !task.IsCompleted)
            {
                EditorGUILayout.HelpBox("Reminder: This task is due soon!", MessageType.Warning);
            }

            // Remove task button
            if (GUILayout.Button("Remove Task"))
            {
                taskManager.Tasks.Remove(task);
            }

            // Draw a colored line under the task details if expanded
            Rect lastRect = GUILayoutUtility.GetLastRect(); // Get the last drawn rect (used for determining the task's box height)
            EditorGUI.DrawRect(new Rect(lastRect.x, lastRect.yMax + 5, lastRect.width, 5), statusColor); // 5px height line

            // Add a space to separate the collapsible content
            EditorGUILayout.Space(5);
        }
        else
        {
            // Draw a colored line under the task details if expanded
            Rect lastRect = GUILayoutUtility.GetLastRect(); // Get the last drawn rect (used for determining the task's box height)
            EditorGUI.DrawRect(new Rect(lastRect.x, lastRect.yMax + 5, lastRect.width, 5), statusColor); // 5px height line

            // Add a space to separate the collapsible content
            EditorGUILayout.Space(5);
        }

        // End the vertical box for the task
        EditorGUILayout.EndVertical();
    }







    private void ShowTaskStatistics()
    {
        int totalTasks = taskManager.Tasks.Count;
        int completedTasks = taskManager.Tasks.Count(t => t.IsCompleted);
        int pendingTasks = totalTasks - completedTasks;

        EditorGUILayout.LabelField("Task Statistics", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Total Tasks: {totalTasks}");
        EditorGUILayout.LabelField($"Completed Tasks: {completedTasks}");
        EditorGUILayout.LabelField($"Pending Tasks: {pendingTasks}");
    }

    private void ExportToJson()
    {
        string json = JsonUtility.ToJson(taskManager);
        string path = EditorUtility.SaveFilePanel("Save Tasks", "", "tasks.json", "json");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            EditorUtility.DisplayDialog("Export Successful", "Tasks have been exported to JSON.", "OK");
        }
    }

    private void ImportFromJson()
    {
        string path = EditorUtility.OpenFilePanel("Open Tasks", "", "json");
        if (!string.IsNullOrEmpty(path))
        {
            string json = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(json, taskManager);
            EditorUtility.DisplayDialog("Import Successful", "Tasks have been imported from JSON.", "OK");
        }
    }

    private void SaveTasks()
    {
        string json = JsonUtility.ToJson(taskManager);
        EditorPrefs.SetString("TaskManagerData", json);
    }

    private void LoadTasks()
    {
        // Attempt to load the saved JSON data from EditorPrefs
        string json = EditorPrefs.GetString("TaskManagerData", "");

        // Check if the JSON string is not empty
        if (!string.IsNullOrEmpty(json))
        {
            // Ensure that taskManager is initialized
            if (taskManager == null)
            {
                taskManager = CreateInstance<TaskManager>();
            }

            // Deserialize the JSON data into the taskManager instance
            JsonUtility.FromJsonOverwrite(json, taskManager);
        }
        else
        {
            // If no saved data, create a new instance of TaskManager
            taskManager = CreateInstance<TaskManager>();
        }
    }

}
