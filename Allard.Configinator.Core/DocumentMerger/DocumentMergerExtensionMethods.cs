namespace Allard.Configinator.Core.DocumentMerger
{
    public static class DocumentMergerExtensionMethods
    {
        public static bool AssignedExplicitValue(this Transition transition) =>
            transition == Transition.Set
            || transition == Transition.SetToSameValue;
    }
}