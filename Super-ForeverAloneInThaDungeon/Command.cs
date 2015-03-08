using System;

namespace Super_ForeverAloneInThaDungeon
{
    public partial class Game
    {
        void ReadCommand()
        {
            DisplayItem dispItem = (new PopupWindowEnterText(40, "Enter command")).item;
            Console.ForegroundColor = ConsoleColor.Green;
            string cmdText = Console.ReadLine();

            string[] args = cmdText.Split(' ');
            string command = args[0].ToUpper();

            if (args.Length > 1)
            {
                string[] newArgs = new string[args.Length - 1];
                for (int i = newArgs.Length; i > 0; i--)
                {
                    newArgs[i - 1] = args[i];
                }
                args = newArgs;
            }
            else
            {
                args = new string[0];
            }

            switch (command)
            {
                case "EXIT":
                    Environment.Exit(0);
                    break;
                case "SUICIDE":
                    OnPlayerDeath();
                    break;
            }

            wipeDisplayItem(dispItem);
            _drawer.Draw(map, false, _playerPosition, currentFloor, getDungeonAt(_playerPosition),
                string.IsNullOrEmpty(_informationMessages) ? GetDefaultInformationMessage() : _informationMessages);
            _informationMessages = string.Empty;
        }
    }
}
