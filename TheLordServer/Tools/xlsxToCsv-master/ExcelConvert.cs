using System;
using System.IO;
using Microsoft.Office.Interop.Excel;

namespace xlsxToCsv
{
    public class ExcelConvert
    {
        [System.Runtime.InteropServices.DllImport ( "user32.dll", SetLastError = true )]
        static extern uint GetWindowThreadProcessId ( IntPtr hWnd, out uint lpdwProcessId );

        const string Exepctional = "#";
        const string IgnoreString = "design";

        public bool Convert(string src, string tar)
        {
            bool result = false;
            Application application = null;
            Workbook workBook = null;
            try
            {
                application = new Application ( );
                workBook = application.Workbooks.Open ( Path.Combine ( Directory.GetCurrentDirectory ( ), src.Substring ( 2 ) ) );
                string table = "";

                for ( int i = 1; i <= workBook.Worksheets.Count; ++i )
                {
                    Worksheet workSheet = workBook.Worksheets.Item[i];
                    if ( workSheet.Name.Contains ( Exepctional ) )   // #가 포함일 경우 제외
                    {
                        continue;
                    }
                    Range range = workSheet.UsedRange;

                    table += string.Format ( "//{0}\n", workSheet.Name );
                    for ( int row = 1; row <= range.Rows.Count; ++row )
                    {
                        for ( int column = 1; column <= range.Columns.Count; ++column )
                        {
                            Range check = ( range.Cells[column][2] as Range );
                            string design = string.Format ( "{0}", check.Value2 );
                            if ( design.Contains ( IgnoreString ) )   // design열 제외
                            {
                                continue;
                            }

                            Range r = ( range.Cells[column][row] as Range );
                            table += string.Format ( "{0}", r.Value2 );
                            if ( column < range.Columns.Count )
                                table += ",";
                        }
                        table = System.Text.RegularExpressions.Regex.Replace ( table, ",*$", "" );
                        table += "\n";
                    }
                    table += "\n";
                }

                string output = tar;
                StreamWriter csv = new StreamWriter ( @output, false );
                csv.Write ( table );
                csv.Close ( );

                
                result = true;
            }
            catch ( Exception e )
            {
                Console.Write ( e.Message );
            }
            finally
            {
                workBook.Close ( true );

                uint processId = 0;
                GetWindowThreadProcessId ( new IntPtr ( application.Hwnd ), out processId );
                application.Quit ( );
                if ( processId != 0 )
                {
                    System.Diagnostics.Process excelProcess = System.Diagnostics.Process.GetProcessById ( (int)processId );
                    excelProcess.CloseMainWindow ( );
                    excelProcess.Refresh ( );
                    excelProcess.Kill ( );
                }
            }

            return result;
        }
    }
}