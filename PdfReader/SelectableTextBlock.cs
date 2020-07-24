using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace PdfReader
{
    public class SelectableTextBlock : TextBlock
    {
        static SelectableTextBlock()
        {
            FocusableProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata(true));
            TextEditorWrapper.RegisterCommandHandlers(typeof(SelectableTextBlock), true, true, true);

            // remove the focus rectangle around the control
            FocusVisualStyleProperty.OverrideMetadata(typeof(SelectableTextBlock),
                new FrameworkPropertyMetadata((object) null));
        }

        protected readonly TextEditorWrapper EditorWrapper;

        public SelectableTextBlock()
        {
            EditorWrapper = TextEditorWrapper.CreateFor(this);
        }
    }

    public class TextEditorWrapper
    {
        private static readonly Type TextEditorType =
            typeof(TextBlock).Assembly.GetType("System.Windows.Documents.TextEditor");

        private static readonly PropertyInfo IsReadOnlyProp =
            TextEditorType.GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly PropertyInfo TextViewProp =
            TextEditorType.GetProperty("TextView", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo RegisterMethod = TextEditorType.GetMethod("RegisterCommandHandlers",
            BindingFlags.Static | BindingFlags.NonPublic, null,
            new[] {typeof(Type), typeof(bool), typeof(bool), typeof(bool)}, null);

        private static readonly Type TextContainerType =
            typeof(TextBlock).Assembly.GetType("System.Windows.Documents.ITextContainer");

        private static readonly PropertyInfo TextContainerTextViewProp = TextContainerType.GetProperty("TextView");

        private static readonly PropertyInfo TextContainerProp =
            typeof(TextBlock).GetProperty("TextContainer", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly,
            bool registerEventListeners)
        {
            RegisterMethod.Invoke(null,
                new object[] {controlType, acceptsRichContent, readOnly, registerEventListeners});
        }

        public static TextEditorWrapper CreateFor(TextBlock tb)
        {
            var textContainer = TextContainerProp.GetValue(tb);
            var editor = new TextEditorWrapper(textContainer, tb, false);
            TextViewProp.SetValue(editor.Editor, TextContainerTextViewProp.GetValue(textContainer));
            return editor;
        }

        public readonly object Editor;

        public TextEditorWrapper(object textContainer, FrameworkElement uiScope, bool isUndoEnabled)
        {
            Editor = Activator.CreateInstance(TextEditorType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null, new[] {textContainer, uiScope, isUndoEnabled}, null);
        }
    }
}