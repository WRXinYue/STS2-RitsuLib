using Godot;
using STS2RitsuLib.Data.Models;

namespace STS2RitsuLib.Settings
{
    internal static class CardPngExportFolderDialog
    {
        internal static void Show(
            ModSettingsValueBinding<RitsuLibSettings, string> outputDirBinding,
            IModSettingsUiActionHost uiHost)
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree?.Root == null)
            {
                RitsuLibFramework.Logger.Warn(
                    "[CardPngExport] Cannot open folder dialog: SceneTree root is not available.");
                return;
            }

            var dialog = new FileDialog
            {
                Title = ModSettingsLocalization.Get("ritsulib.cardPngExport.browseTitle",
                    "Choose card PNG export folder"),
                FileMode = FileDialog.FileModeEnum.OpenDir,
                Access = FileDialog.AccessEnum.Filesystem,
            };

            dialog.DirSelected += path =>
            {
                outputDirBinding.Write(path);
                outputDirBinding.Save();
                uiHost.RequestRefresh();
                dialog.QueueFree();
            };
            dialog.Canceled += dialog.QueueFree;

            tree.Root.AddChild(dialog);
            dialog.PopupCenteredRatio(0.55f);
        }
    }
}
