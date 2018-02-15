using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DankBot
{
    class RextesterHelper
    {
        public static Dictionary<string, int> languages = new Dictionary<string, int>{
            { "ada", 39 },
            { "assembly", 15 },
            { "bash", 38 },
            { "brainfuck", 44 },
            { "c#", 1 },
            { "cpp_gcc", 7 },
            { "cpp_clang", 27 },
            { "cpp_vc++", 28 },
            { "c_gcc", 6 },
            { "c_clang", 26 },
            { "c_vc", 29 },
            { "clientside", 36 },
            { "commonlist", 18 },
            { "d", 30 },
            { "elixir", 41 },
            { "erlang", 40 },
            { "f#", 3 },
            { "fortan", 45 },
            { "go", 20 },
            { "haskell", 11 },
            { "java", 4 },
            { "javascript", 17 },
            { "kotlin", 43 },
            { "lua", 14 },
            { "mysql", 33 },
            { "nodejs", 23 },
            { "ocaml", 42 },
            { "octave", 25 },
            { "objectivec", 10 },
            { "oracle", 35 },
            { "pascal", 9 },
            { "perl", 13 },
            { "php", 8 },
            { "postgresql", 34 },
            { "prolog", 19 },
            { "python", 5 },
            { "python3", 24 },
            { "r", 31 },
            { "ruby", 12 },
            { "scala", 21 },
            { "scheme", 22 },
            { "sqlserver", 16 },
            { "swift", 37 },
            { "tcl", 32 },
            { "visualbasic", 2 },
        };

        public static Dictionary<int, string> compiler_args = new Dictionary<int, string>{
            { 7, @"-Wall -std=c++14 -O2 -o a.out source_file.cpp" },
            { 27, @"-Wall -std=c++14 -O2 -o a.out source_file.cpp" },
            { 28, @"source_file.cpp -o a.exe /EHsc /MD /I C:\boost_1_60_0 /link /LIBPATH:C:\boost_1_60_0\stage\lib" },
            { 6, @"-Wall -std=gnu99 -O2 -o a.out source_file.c" },
            { 26, @"-Wall -std=gnu99 -O2 -o a.out source_file.c" },
            { 29, @"source_file.c -o a.exe" },
            { 30, @"source_file.d -ofa.out" },
            { 20, @"-o a.out source_file.go" },
            { 11, @"-o a.out source_file.hs" },
            { 10, @"-MMD -MP -DGNUSTEP -DGNUSTEP_BASE_LIBRARY=1 -DGNU_GUI_LIBRARY=1 -DGNU_RUNTIME=1 -DGNUSTEP_BASE_LIBRARY=1 -fno-strict-aliasing -fexceptions -fobjc-exceptions -D_NATIVE_OBJC_EXCEPTIONS -pthread -fPIC -Wall -DGSWARN -DGSDIAGNOSE -Wno-import -g -O2 -fgnu-runtime -fconstant-string-class=NSConstantString -I. -I /usr/include/GNUstep -I/usr/include/GNUstep -o a.out source_file.m -lobjc -lgnustep-base" },
        };

        public static async Task<RextesterResponse> runCodeAsync(string prog, int language)
        {
            string cargs = "";
            compiler_args.TryGetValue(language, out cargs);

            var values = new Dictionary<string, string>
            {
                { "LanguageChoiceWrapper", language.ToString() }, // 1 C#, 5 Python
                { "EditorChoiceWrapper", "1" },
                { "LayoutChoiceWrapper", "1" },
                { "Program", prog },
                { "ShowWarnings", "false" },
                { "IsInEditMode", "false" },
                { "IsLive", "false" },
                { "CompilerArgs", cargs },
                { "Input", "" }
            };

            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 10);

            var content = new FormUrlEncodedContent(values);

            try
            {
                var httpresponse = await (await client.PostAsync("http://rextester.com/rundotnet/Run", content)).Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RextesterResponse>(httpresponse);
            }
            catch
            {
                RextesterResponse response = new RextesterResponse();
                response.Errors = "Code ran longer than 10 seconds !";
                response.Stats = "";
                response.Result = "";
                return response;
            }
        }


    }

    class RextesterResponse
    {
        public string Warnings;
        public string Errors;
        public string Result;
        public string Stats;
        public string Files;
    }
}
