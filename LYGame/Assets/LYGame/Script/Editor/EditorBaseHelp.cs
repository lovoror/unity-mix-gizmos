using UnityEngine;
using UnityEditor;

namespace LYGame.Editor
{
	public enum ForeachSelectionResult
	{
		result_none,
		result_continue,
		result_break,
	}

	public class EditorBaseHelp
	{
		public delegate ForeachSelectionResult SelectionHandler(UnityEngine.Object obj);

		public static void ForeachSelection(SelectionHandler handler)
		{
			foreach (UnityEngine.Object obj in Selection.objects)
			{
				ForeachSelectionResult result = handler.Invoke(obj);
				if (result == ForeachSelectionResult.result_continue)
					continue;
				else if (result == ForeachSelectionResult.result_continue)
					break;
			}
		}
	}
}
