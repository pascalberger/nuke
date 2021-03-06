﻿// Copyright Matthias Koch 2017.
// Distributed under the MIT License.
// https://github.com/matkoch/Nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Nuke.Core.Execution
{
    public class TargetDefinition : ITargetDefinition
    {
        public static TargetDefinition Create(string name, Target factory = null)
        {
            return new TargetDefinition(name, factory);
        }

        private TargetDefinition (string name, Target factory = null)
        {
            Name = name;
            Factory = factory;
            DependentTargets = new List<Target>();
            DependentShadowTargets = new List<string>();
            Actions = new List<Action>();
            Conditions = new List<Func<bool>>();

            factory?.Invoke(this);
        }

        internal string Name { get; }
        [CanBeNull]
        internal Target Factory { get; }
        internal TimeSpan Duration { get; set; }
        internal ExecutionStatus Status { get; set; }
        internal List<Func<bool>> Conditions { get; }
        internal List<Target> DependentTargets { get; }
        internal List<string> DependentShadowTargets { get; }
        internal List<Action> Actions { get; }

        public ITargetDefinition Executes (params Action[] actions)
        {
            Actions.AddRange(actions);
            return this;
        }

        public ITargetDefinition Executes<T> (Func<T> action)
        {
            return Executes(new Action(() => action()));
        }

        public ITargetDefinition DependsOn (params Target[] targets)
        {
            DependentTargets.AddRange(targets);
            return this;
        }

        public ITargetDefinition DependsOn (params string[] shadowTargets)
        {
            DependentShadowTargets.AddRange(shadowTargets);
            return this;
        }

        public ITargetDefinition OnlyWhen (params Func<bool>[] conditions)
        {
            Conditions.AddRange(conditions);
            return this;
        }

        public override string ToString ()
        {
            return $"Target '{Name}'";
        }
    }
}
