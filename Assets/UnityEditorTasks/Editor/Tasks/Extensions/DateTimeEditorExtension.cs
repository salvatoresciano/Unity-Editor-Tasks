using System;
using UnityEditor;

public static class DateTimeEditorExtension
{
    public static DateTime DateField(string label, DateTime date)
    {
        // Convert DateTime to string for display
        string dateString = date.ToString("yyyy-MM-dd");
        // Use a TextField to accept user input
        string newDateString = EditorGUILayout.TextField(label, dateString);

        // Try parsing the new input, default to the original date if parsing fails
        if (DateTime.TryParse(newDateString, out var newDate))
        {
            return newDate;
        }
        return date;
    }

    public static string DateFieldToString(string label, string date)
    {
   
        // Use a TextField to accept user input
        string newDateString = EditorGUILayout.TextField(label, date);

        return newDateString;
    }
}
