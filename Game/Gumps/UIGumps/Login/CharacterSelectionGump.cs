﻿using ClassicUO.Game.Gumps.Controls;
using ClassicUO.Game.Scenes;
using ClassicUO.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ClassicUO.Input;

namespace ClassicUO.Game.Gumps.UIGumps.Login
{
    class CharacterSelectionGump : Gump
    {
        private const ushort SELECTED_COLOR = 0x0021;
        private const ushort NORMAL_COLOR = 0x034F;
        private byte _selectedCharacter = -1;

        public CharacterSelectionGump()
            : base(0, 0)
        {
            bool testField = FileManager.ClientVersion >= ClientVersions.CV_305D;
            int posInList = 0;
            int yOffset = 150;
            int yBonus = 0;
            int listTitleY = 106;

            if (FileManager.ClientVersion >= ClientVersions.CV_6040)
            {
                listTitleY = 96;
                yOffset = 125;
                yBonus = 45;
            }


            AddChildren(new ResizePic(0x0A28) { X = 160, Y = 70, Width = 408, Height = 343 + yBonus });
            AddChildren(new Label(IO.Resources.Cliloc.GetString(3000050), false, 0x0386, font: 2) { X = 267, Y = listTitleY });

            var loginScene = Service.Get<LoginScene>();
            foreach(var character in loginScene.Characters) {
                AddChildren(new CharacterEntryGump(character, posInList)
                {
                    X = 224,
                    Y = yOffset + (posInList * 40),
                    Hue = posInList == 0 ? SELECTED_COLOR : NORMAL_COLOR
                });

                posInList++;
            }


            if (loginScene.Characters.Any(o => string.IsNullOrEmpty(o.Name)))
                AddChildren(new Button((int)Buttons.New, 0x159D, 0x159F, over: 0x159E) { X = 224, Y = 350 + yBonus });

            AddChildren(new Button((int)Buttons.Delete, 0x159A, 0x159C, over: 0x159B) { X = 442, Y = 350 + yBonus });
            AddChildren(new Button((int)Buttons.Prev, 0x15A1, 0x15A3, over: 0x15A2) { X = 586, Y = 445, ButtonAction = ButtonAction.Activate });
            AddChildren(new Button((int)Buttons.Next, 0x15A4, 0x15A6, over: 0x15A5) { X = 610, Y = 445, ButtonAction = ButtonAction.Activate });

            if (loginScene.Characters.Length > 0)
                _selectedCharacter = 0;
        }

        public override void OnButtonClick(int buttonID)
        {
            var loginScene = Service.Get<LoginScene>();

            if (buttonID >= (int)Buttons.Character)
            {
                var index = buttonID - (int)Buttons.Character;
                if (_selectedCharacter == index)
                    loginScene.SelectCharacter((byte)index, loginScene.Characters[index].Name);
                else
                {
                    _selectedCharacter = (byte)index;

                    foreach(var characterGump in GetControls<CharacterEntryGump>())
                    {
                        if (characterGump.CharacterIndex == index)
                            characterGump.Hue = SELECTED_COLOR;
                        else
                            characterGump.Hue = NORMAL_COLOR;
                    }
                }
            } else
            {
                switch((Buttons)buttonID)
                {
                    case Buttons.Next:
                        if (loginScene.Characters.Count() > 0)
                            loginScene.SelectCharacter(_selectedCharacter, loginScene.Characters[_selectedCharacter].Name);
                        break;
                    case Buttons.Prev:
                        loginScene.StepBack();
                        break;
                }
            }

            base.OnButtonClick(buttonID);
        }

        private enum Buttons
        {
            New, Delete, Next, Prev, Character = 99
        }

        private class CharacterEntryGump : GumpControl
        {
            private Label _label;
            public int CharacterIndex { get; private set; }

            public ushort Hue
            {
                get => _label.Hue;
                set => _label.Hue = value;
            }

            public CharacterEntryGump(CharacterListEntry character, int index)
            {
                CharacterIndex = index;

                // Bg
                AddChildren(new ResizePic(0x0BB8) { X = 0, Y = 0, Width = 280, Height = 30 });

                // Char Name
                AddChildren(_label = new Label(character.Name, false, NORMAL_COLOR, 270, 5, align: IO.Resources.TEXT_ALIGN_TYPE.TS_CENTER)
                {
                    X = 0
                });

                AcceptMouseInput = true;
            }
            
            protected override void OnMouseClick(int x, int y, MouseButton button)
            {
                if (button == MouseButton.Left)
                {
                    OnButtonClick((int)Buttons.Character + CharacterIndex);
                }
            }
        }
    }
}