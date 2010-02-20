using System;
using System.Linq;
namespace DemoGame.DbObjs
{
/// <summary>
/// Interface for a class that can be used to serialize values to the database table `quest_require_start_item`.
/// </summary>
public interface IQuestRequireStartItemTable
{
/// <summary>
/// Creates a deep copy of this table. All the values will be the same
/// but they will be contained in a different object instance.
/// </summary>
/// <returns>
/// A deep copy of this table.
/// </returns>
IQuestRequireStartItemTable DeepCopy();

/// <summary>
/// Gets the value of the database column `amount`.
/// </summary>
System.Byte Amount
{
get;
}
/// <summary>
/// Gets the value of the database column `item_template_id`.
/// </summary>
DemoGame.ItemTemplateID ItemTemplateID
{
get;
}
/// <summary>
/// Gets the value of the database column `quest_id`.
/// </summary>
NetGore.Features.Quests.QuestID QuestID
{
get;
}
}

}