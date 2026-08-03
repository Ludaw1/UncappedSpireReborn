using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace UncappedSpireReborn.UncappedSpireRebornCode.UncappedActs;

public static class UncappedActsEntry
{
    public static async Task EnterPostActThreeBossRoom()
    {
        var runManager = RunManager.Instance;
        if (UncappedActsConfig.UncappedActsEnabled)
        {
            await runManager.EnterRoom(new EventRoom(ModelDb.Event<ClosingTheChapterEvent>()));
        }
        else
        {
            await runManager.EnterRoom(new EventRoom(ModelDb.Event<TheArchitect>()));
        }
    }
}
