﻿using System;
using System.Collections.Generic;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Lamar.IoC.Frames;

namespace Lamar.IoC.Instances
{
    public class InlineLambdaCreationFrame<TContainer> : SyncFrame
    {
        private Variable _scope;
        private readonly Setter _setter;

        public InlineLambdaCreationFrame(Setter setter, Instance instance)
        {
            Variable = new ServiceVariable(instance, this);
            _setter = setter;
        }
        
        public ServiceVariable Variable { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Variable.Usage} = ({Variable.VariableType.FullNameInCode()}){_setter.Usage}(({typeof(TContainer).FullNameInCode()}){_scope.Usage});");

            if(!Variable.VariableType.IsPrimitive && Variable.VariableType != typeof(string))
            {
                writer.Write($"var {Variable.Usage}_disposable = {Variable.Usage} as System.IDisposable;");
                writer.Write($"if({Variable.Usage}_disposable != null) {{");
                writer.Write($"   {_scope.Usage}.Disposables.Add({Variable.Usage}_disposable);");
                writer.Write("}");
            }

            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            yield return _setter;
            _scope = chain.FindVariable(typeof(Scope));
            yield return _scope;
        }
    }
}