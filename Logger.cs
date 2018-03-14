/*
 * Created by SharpDevelop.
 * User: NCDyson
 * Date: 1/6/2018
 * Time: 5:49 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.Windows.Forms;

namespace RareView
{
	/// <summary>
	/// Description of Logger.
	/// </summary>
	public static class Logger
	{
		public static RichTextBox LogControl = null;
		
		
		public static void SetLogControl(RichTextBox r)
		{
			LogControl = r;
		}
		
		public static void LogError(string errorText)
		{
			LogGeneric(errorText, Color.Red);
		}
		
		public static void LogWarning(string warningText)
		{
			LogGeneric(warningText, Color.Green);
		}
		
		public static void LogInfo(string infoText)
		{
			LogGeneric(infoText, Color.White);
		}
		
		public static void LogGeneric(string text, Color textColor)
		{
			if(LogControl == null) return;
			
			Color oldColor = LogControl.SelectionColor;
			LogControl.SelectionColor = textColor;
			LogControl.AppendText(text);
			LogControl.SelectionColor = oldColor;
		}
	}
}
