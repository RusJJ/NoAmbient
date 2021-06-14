using BepInEx;
using Bepinject;
using ComputerInterface;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using System;
using System.Text;
using NoAmbientMod;

namespace NoAmbient_Computer
{
    /* That's me! */
    [BepInPlugin("net.rusjj.noambient.ci", "No Ambient Sounds: CI", "1.0.0")]
    [BepInDependency("net.rusjj.noambient", "1.0.0")]
    [BepInDependency("tonimacaroni.computerinterface", "1.4.4")]

    public class NoAmbient_Computer : BaseUnityPlugin
    {
        void Awake()
        {
            Zenjector.Install<MainInstaller>().OnProject();
        }
    }
    internal class MainInstaller : Zenject.InstallerBase
    {
        public override void InstallBindings()
        {
            Container.Bind<IComputerModEntry>().To<NoAmbientEntry>().AsSingle();
        }
    }

    public class NoAmbientView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;
        private NoAmbientView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
            _selectionHandler.OnSelected += OnOptionSelected;
            _selectionHandler.MaxIdx = 2;
        }
        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }
        private void OnOptionSelected(int idx)
        {
            switch(idx)
            {
                case 0:
                    NoAmbient.SetActiveForest(!NoAmbient.IsActiveForest());
                    break;

                case 1:
                    NoAmbient.SetActiveCave(!NoAmbient.IsActiveCave());
                    break;

                case 2:
                    NoAmbient.SetActiveCanyon(!NoAmbient.IsActiveCanyon());
                    break;
            }
            Redraw();
        }
        private void Redraw()
        {
            var sb = new StringBuilder();
            sb.Append("/// ").Append("Choose ambient sound to toggle it").Append(" ///").AppendLine().AppendLine();

            sb.Append(NoAmbient.IsActiveForest() ? "<color=#2ca600>" : "<color=#ff3700>");
            sb.Append(_selectionHandler.CurrentSelectionIndex == 0 ? "> " : "  ");
            sb.Append("Forest Ambient").AppendLine();

            sb.Append(NoAmbient.IsActiveCave() ? "<color=#2ca600>" : "<color=#ff3700>");
            sb.Append(_selectionHandler.CurrentSelectionIndex == 1 ? "> " : "  ");
            sb.Append("Cave Ambient").AppendLine();

            sb.Append(NoAmbient.IsActiveCanyon() ? "<color=#2ca600>" : "<color=#ff3700>");
            sb.Append(_selectionHandler.CurrentSelectionIndex == 2 ? "> " : "  ");
            sb.Append("Canyon Ambient");

            Text = sb.ToString();
        }
        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                Redraw();
                return;
            }
            switch (key)
            {
                case EKeyboardKey.Back:
                case EKeyboardKey.Delete:
                    ReturnToMainMenu();
                    break;
            }
        }
    }
    public class NoAmbientEntry : IComputerModEntry
    {
        public string EntryName => "NoAmbient Settings";
        public Type EntryViewType => typeof(NoAmbientView);
    }
}