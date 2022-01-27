using SteamKit2;
using SteamKit2.Internal;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace hide_recent_activity

{
    class Program
    {
        static SteamClient steamClient;
        static SteamUser steamUser;
        static CallbackManager manager;
        static bool isRunning = false;

        static string username;
        static string password;
        static string authCode;
        static string twoFactorAuth;

        static void Main(string[] args)
        {
            Console.Title = "drain gang license product";
            Console.WindowHeight = 20;
            Console.WindowWidth = 65;
            Console.WriteLine(@"
    ░░░▒░░░░╫▐C '▒▒░   ▓     ░▓▓▓▒▓▒▒░░▒╨▓▓▓
    ░▒'    ╒`▓▓m       ╟▒   ¿░╙▓▓▓▒▒░▒░▒░░░░
    `      `▐▓╢▒ ░╖  , ]╣   ░  ▓▓▓▓@░░░░░░░░
          ╣g▓,╢M  ╟▓µ,┐░`  ,░╒▓▓▓▓▓▓▓░░░░░░░
         ╢J]M░╢   ╢║▓▓▓▌   ▓▓▓╣▓▓▓▓▓▓▓▒░▒▒░░
        ]▓,]░ ]░ ░╢▓▓▓▓    ▓▓▓▓▓▓▓▓▓▓▓▓▒▒░░░
       ,▓▌` @  ░░µ▓▓╢▓▓   ▐▓▓▓▓▓▓▓▓╢▓▓▒╣░░▒░
      'V░▓Ç╫▓▓▓░@▓▓▓▓╢▓   ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓╢▒▒
           ╙░░░``╙▓╢╢▓▓  ▐▓▓▓╢╣▓╢╢▓▓▓▓▓╣▓▓,
          ,▄▓`     `▀▓▓  ▓╣▓╜╙╫▓╣╣▓▓▓▓▓▓╣╢∞
        ╓▓▓▓╦╖       'Γ ]▀       ╙▓▓▓╣╣╢▓╜ ╓
      .▓▓▓▓╣╣╣▒@     r  ""           ▒▀▒` ▄▓▓
      ▓▓▓▓▓▓▓▓▓▓▌  ░  ,▓▓@H,      ,▒░▄▓▓▓▓▓▓
    .▐▓▓▓▓▓▓▓▓█▀""░  ^g▓▓▓▓▓▓▌,╓▄▓▓▓▓▓▓▓▓▀╜
      ▀▓▓▓▓█▀░░     ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▀`
                   ▓▓▓▓▓╢▓▓▓▓▓▓▓▓▓▀╜
                  j▓▓▓▓▓▓▓▓▓▓▓▀▀
                   ╙▓▓▓▓▀▀`");
            Console.WriteLine("\nCredits: vro, deep, sdev, Fermion");
            Console.WriteLine("Credits: jake, fassu, wilko, konan, visualbug, cony");
            Console.WriteLine("Credits: shiio, Skile, LordShadowBluRay, lexi, kovia, pls");
            Console.WriteLine("Credits: yuzumi, sora, pearl, misfires, hana, alina, don cristii\n");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("> Login: "); username = Console.ReadLine();
            Console.Write("> Password: "); password = ReadPassword();

            steamClient = new SteamClient();
            manager = new CallbackManager(steamClient);


            steamUser = steamClient.GetHandler<SteamUser>();

            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            manager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);


            Run();
        }
        static void Run()
        {
            isRunning = true;

            steamClient.Connect();

            while (isRunning)
            {
                manager.RunWaitAllCallbacks(TimeSpan.FromSeconds(5));
            }
        }

        static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))
            {

                byte[] sentryFile = File.ReadAllBytes("sentry.bin");
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            try
            {
                steamUser.LogOn(new SteamUser.LogOnDetails
                {
                    Username = username,
                    Password = password,
                    LoginID = 1,
                    AuthCode = authCode,
                    TwoFactorCode = twoFactorAuth,
                    SentryFileHash = sentryHash
                });
            }
            catch (ArgumentException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Don't ignore input fields.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey();
                Environment.Exit(-1);
            }

        }

        static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {

            Console.WriteLine("[+] Reconnecting in 5 secs...");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            steamClient.Connect();


        }


        async static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {

            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2FA = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor;

            if (isSteamGuard || is2FA)
            {
                if (is2FA)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[+] Your 2 factor auth code: ");
                    twoFactorAuth = Console.ReadLine();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[+] Auth code sent to the email: ");
                    authCode = Console.ReadLine();
                }
                return;


            }

            if (callback.Result != EResult.OK)
            {
                if (callback.Result == EResult.RateLimitExceeded)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[-] RateLimitExceeded");
                    isRunning = false;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                    return;
                }

                if (callback.Result == EResult.ServiceUnavailable)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[-] Steam down");
                    isRunning = false;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                    return;
                }

                if (callback.Result == EResult.InvalidPassword)
                {

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[-] Invalid password or login");
                    isRunning = false;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                    return;
                }

                if (callback.Result == EResult.InvalidLoginAuthCode)
                {

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[-] Invalid auth code");
                    isRunning = false;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                    return;
                }

                if (callback.Result == EResult.TwoFactorCodeMismatch)
                {

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[-] Invalid 2 factor auth code");
                    isRunning = false;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[-] Failed to login due to: " + callback.Result);
                isRunning = false;
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey();
                return;


            }
            else
            {
                Console.WriteLine("[+] Steam Welcome");
                Console.WriteLine("[+] logged on");
            }

            uint[] arr = { 635240, 635241, 635242, 635243 };
            var steamApps = steamClient.GetHandler<SteamApps>();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[+] calling shit...\n");
            foreach (uint result in arr)
            {
                var depotJob = steamApps.RequestFreeLicense(result);
                SteamApps.FreeLicenseCallback depotKey = await depotJob;
                if (depotKey.Result == EResult.OK)
                {
                    Console.ForegroundColor = ConsoleColor.Green;


                    var gamesPlaying = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

                    foreach (ulong game in arr)
                    {
                        Console.WriteLine("[+] JobID " + Convert.ToDecimal(depotKey.JobID) + " with " + game + " Send " + depotKey.Result);
                        gamesPlaying.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
                        {
                            game_id = new GameID(game)
                        });
                        steamClient.Send(gamesPlaying);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[-] " + depotKey.Result);
                    Console.ReadKey();
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nRecent Activity Clean! :3");
            isRunning = false;
            Thread.Sleep(2000);
            Environment.Exit(-1);
        }


        static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {

            Console.WriteLine($"Logged off of Steam: {callback.Result}");
            isRunning = false;
            return;
        }

        static void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {

            int fileSize;
            byte[] sentryHash;
            using (var fs = File.Open("sentry.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Seek(callback.Offset, SeekOrigin.Begin);
                fs.Write(callback.Data, 0, callback.BytesToWrite);
                fileSize = (int)fs.Length;

                fs.Seek(0, SeekOrigin.Begin);
                using var sha = SHA1.Create();
                sentryHash = sha.ComputeHash(fs);
            }

            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = fileSize,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });

        }


        public static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {

                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {

                        password = password.Substring(0, password.Length - 1);
                        int pos = Console.CursorLeft;
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        Console.Write(" ");
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }
            Console.WriteLine();
            return password;
        }
    }
}
