using SteamKit2;
using SteamKit2.Authentication;
using SteamKit2.Internal;
using System;
using System.Threading.Tasks;

namespace hide_recent_activity
{
    class Program
    {
        static SteamClient steamClient;
        static SteamUser steamUser;
        static CallbackManager manager;     
        static bool isRunning = true;

        static string username;
        static string password;
        static string guardData = null;

        static void Main(string[] args)
        {
            Console.Title = "drain gang license product";
            Console.WindowHeight = 24;
            Console.WindowWidth = 100;

            Console.ForegroundColor = ConsoleColor.Magenta;
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

            Console.WriteLine("\n          Credits: vro (verdammnis)");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\n> Steam username: ");
            username = Console.ReadLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("> Steam password: ");
            password = ReadPassword();

            steamClient = new SteamClient();
            manager = new CallbackManager(steamClient);
            steamUser = steamClient.GetHandler<SteamUser>();

            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Connecting to Steam...");
            Console.ResetColor();

            steamClient.Connect();

            while (isRunning)
            {
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        static async void OnConnected(SteamClient.ConnectedCallback callback)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[+] Connected to Steam successfully\n");
            Console.ResetColor();

            try
            {
                var authSession = await steamClient.Authentication.BeginAuthSessionViaCredentialsAsync(new AuthSessionDetails
                {
                    Username = username,
                    Password = password,
                    IsPersistentSession = true,
                    GuardData = guardData,
                    Authenticator = new UserConsoleAuthenticator()
                });

                var pollResponse = await authSession.PollingWaitForResultAsync();

                if (pollResponse.NewGuardData != null)
                    guardData = pollResponse.NewGuardData;

                steamUser.LogOn(new SteamUser.LogOnDetails
                {
                    Username = pollResponse.AccountName,
                    AccessToken = pollResponse.RefreshToken,
                    ShouldRememberPassword = true
                });
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[-] Authentication error: " + ex.Message);
                isRunning = false;
            }
        }

        static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[-] Login failed: {callback.Result}");
                isRunning = false;
                Console.ReadKey();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[+] Successfully logged into Steam");
            Console.ResetColor();

            PerformHideActivity();
        }

        static async void PerformHideActivity()
        {
            var steamApps = steamClient.GetHandler<SteamApps>();
            uint[] appIds = { 635240, 635241, 635242, 635243 };

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[+] Requesting free licenses...\n");
            Console.ResetColor();

            foreach (uint appId in appIds)
            {
                var result = await steamApps.RequestFreeLicense(appId);
                if (result.Result == EResult.OK)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[+] License granted → {appId}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[-] Failed → {appId} | {result.Result}");
                }
                Console.ResetColor();
            }

            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[+] Simulating game activity...\n");
            Console.ResetColor();

            var gamesPlaying = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

            for (int i = 0; i < appIds.Length; i++)
            {
                ulong appId = appIds[i];

                gamesPlaying.Body.games_played.Clear();
                gamesPlaying.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
                {
                    game_id = new GameID(appId)
                });

                steamClient.Send(gamesPlaying);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[+] Playing → {appId}");
                Console.ResetColor();

                await Task.Delay(1200);
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n[+] Recent Activity has been successfully cleared!");
            Console.ResetColor();

            await Task.Delay(2500);
            isRunning = false;
            steamUser.LogOff();
        }

        static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n[+] Disconnected from Steam");
            Console.ResetColor();
            isRunning = false;
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
                else if (info.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    int pos = Console.CursorLeft;
                    Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    Console.Write(" ");
                    Console.SetCursorPosition(pos - 1, Console.CursorTop);
                }
                info = Console.ReadKey(true);
            }
            Console.WriteLine();
            return password;
        }
    }
}
