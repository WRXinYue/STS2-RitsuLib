using System.Reflection;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Timeline;

namespace STS2RitsuLib.Diagnostics
{
    internal static class RegistrationConflictDetector
    {
        internal static void ThrowIfModelIdConflicts(Type candidateType)
        {
            ArgumentNullException.ThrowIfNull(candidateType);

            var candidateId = ModelDb.GetId(candidateType);
            var conflicts = ReflectionHelper.GetSubtypes<AbstractModel>()
                .Where(type => type != candidateType)
                .Where(type => ModelDb.GetId(type) == candidateId)
                .ToArray();

            if (conflicts.Length == 0)
                return;

            throw new InvalidOperationException(
                $"ModelId collision detected for '{candidateId}'. Type '{candidateType.FullName}' conflicts with: " +
                string.Join(", ", conflicts.Select(type => type.FullName)) +
                ". STS2 builds ModelId from the model category and the slugified type name, so same-type-name models in the same category will collide.");
        }

        internal static void ValidateAndLogModelIdCollisions()
        {
            var conflicts = ReflectionHelper.GetSubtypes<AbstractModel>()
                .GroupBy(ModelDb.GetId)
                .Where(group => group.Count() > 1)
                .ToArray();

            foreach (var group in conflicts)
                RitsuLibFramework.Logger.Error(
                    $"[Content] ModelId collision detected for '{group.Key}': " +
                    string.Join(", ", group.Select(type => type.FullName)));

            if (conflicts.Length > 0)
                RitsuLibFramework.Logger.Error(
                    "[Content] Duplicate model type names in the same model category are unsafe. The game derives ModelId from the type name, and later registrations may overwrite earlier ones in ModelDb.");
        }

        internal static void ThrowIfEpochIdConflicts(string epochId, Type candidateType,
            IEnumerable<Type> knownEpochTypes)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(epochId);
            ArgumentNullException.ThrowIfNull(candidateType);
            ArgumentNullException.ThrowIfNull(knownEpochTypes);

            var conflicts = knownEpochTypes
                .Where(type => type != candidateType)
                .Where(type => CreateEpoch(type).Id.Equals(epochId, StringComparison.Ordinal))
                .ToArray();

            if (conflicts.Length == 0)
                return;

            throw new InvalidOperationException(
                $"Epoch id collision detected for '{epochId}'. Type '{candidateType.FullName}' conflicts with: " +
                string.Join(", ", conflicts.Select(type => type.FullName)));
        }

        internal static void ThrowIfStoryIdConflicts(string storyId, Type candidateType,
            IEnumerable<Type> knownStoryTypes)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(storyId);
            ArgumentNullException.ThrowIfNull(candidateType);
            ArgumentNullException.ThrowIfNull(knownStoryTypes);

            var conflicts = knownStoryTypes
                .Where(type => type != candidateType)
                .Where(type => GetStoryId(type).Equals(storyId, StringComparison.Ordinal))
                .ToArray();

            if (conflicts.Length == 0)
                return;

            throw new InvalidOperationException(
                $"Story id collision detected for '{storyId}'. Type '{candidateType.FullName}' conflicts with: " +
                string.Join(", ", conflicts.Select(type => type.FullName)));
        }

        private static EpochModel CreateEpoch(Type type)
        {
            return (EpochModel)Activator.CreateInstance(type)!;
        }

        private static string GetStoryId(Type type)
        {
            var story = (StoryModel)Activator.CreateInstance(type)!;
            var property = type.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return (string)(property?.GetValue(story) ??
                            throw new InvalidOperationException(
                                $"Story type '{type.FullName}' does not expose an Id property."));
        }
    }
}
