/********************************************************************
                   DO NOT MANUALLY EDIT THIS FILE!

This file was automatically generated using the DbClassCreator
program. The only time you should ever alter this file is if you are
using an automated code formatter. The DbClassCreator will overwrite
this file every time it is run, so all manual changes will be lost.
If there is something in this file that you wish to change, you should
be able to do it through the DbClassCreator arguments.

Make sure that you re-run the DbClassCreator every time you alter your
game's database.

For more information on the DbClassCreator, please see:
    http://www.netgore.com/wiki/dbclasscreator.html

This file was generated on (UTC): 5/11/2010 11:46:42 PM
********************************************************************/

using System;
using System.Linq;
namespace DemoGame.DbObjs
{
/// <summary>
/// Interface for a class that can be used to serialize values to the database table `quest_require_finish_quest`.
/// </summary>
public interface IQuestRequireFinishQuestTable
{
/// <summary>
/// Creates a deep copy of this table. All the values will be the same
/// but they will be contained in a different object instance.
/// </summary>
/// <returns>
/// A deep copy of this table.
/// </returns>
IQuestRequireFinishQuestTable DeepCopy();

/// <summary>
/// Gets the value of the database column `quest_id`.
/// </summary>
NetGore.Features.Quests.QuestID QuestID
{
get;
}
/// <summary>
/// Gets the value of the database column `req_quest_id`.
/// </summary>
NetGore.Features.Quests.QuestID ReqQuestID
{
get;
}
}

}
