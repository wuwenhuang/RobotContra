#region File Description
//-----------------------------------------------------------------------------
// MultiplayerMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class MultiplayerMenuScreen : MenuScreen
    {
        #region Fields

        MenuEntry createSessionMenuEntry;
        MenuEntry joinSessionMenuEntry;
        MenuEntry backMenuEntry;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public MultiplayerMenuScreen()
            : base("Multiplayer")
        {

            // Create our menu entries.
            createSessionMenuEntry = new MenuEntry("Create Session");
            joinSessionMenuEntry = new MenuEntry("Join Session");
            backMenuEntry = new MenuEntry("Back");

            // Hook up menu event handlers.
            createSessionMenuEntry.Selected += CreateSessionMenuSelected;
            joinSessionMenuEntry.Selected += JoinSessionMenuSelected;
            backMenuEntry.Selected += BackMenuSelected;

            // Add entries to the menu.
            MenuEntries.Add(createSessionMenuEntry);
            MenuEntries.Add(joinSessionMenuEntry);
            MenuEntries.Add(backMenuEntry);
        }


        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>

        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Ungulate menu entry is selected.
        /// </summary>
        void CreateSessionMenuSelected(object sender, PlayerIndexEventArgs e)
        {
            // create session
        }


        /// <summary>
        /// Event handler for when the Language menu entry is selected.
        /// </summary>
        void JoinSessionMenuSelected(object sender, PlayerIndexEventArgs e)
        {
            // join session
        }

        void BackMenuSelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new MainMenuScreen(), e.PlayerIndex);
        }

        #endregion
    }
}
