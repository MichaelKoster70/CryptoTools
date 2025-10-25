// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CommandLine.Text;

namespace CertTools.CertCore;

/// <summary>
/// Static class holding extension methods for the command line parser.
/// </summary>
public static class OptionsExtensions
{
   /// <summary>
   /// Prints the tool heading information (same as Parser)
   /// </summary>
   public static void PrintToolInfo()
   {
      Console.WriteLine(HeadingInfo.Default);
      Console.WriteLine(CopyrightInfo.Default);
      Console.WriteLine();
   }
}