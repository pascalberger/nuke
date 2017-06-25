using System;
using System.Linq;
using Nuke.ToolGenerator.Model;

namespace Nuke.ToolGenerator.Writers
{
    public class TaskWriter : IWriterWrapper
    {
        public TaskWriter (Task task, ToolWriter toolWriter)
        {
            Tool = toolWriter.Tool;
            Task = task;
            Writer = toolWriter;
        }

        public Tool Tool { get; }
        public Task Task { get; }
        public IWriter Writer { get; }
    }
}
