// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Rendering;

namespace Microsoft.AspNetCore.Components.Test.Helpers
{
    public class TestRenderer : Renderer
    {
        public TestRenderer() : this(new TestServiceProvider())
        {
        }

        public TestRenderer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public Action<RenderBatch> OnUpdateDisplay { get; set; }

        public List<CapturedBatch> Batches { get; }
            = new List<CapturedBatch>();

        public List<(int componentId, Exception exception)> HandledExceptions { get; }
            = new List<(int, Exception)>();

        public bool ShouldHandleExceptions { get; set; }

        public new int AssignRootComponentId(IComponent component)
            => base.AssignRootComponentId(component);

        public new void RenderRootComponent(int componentId)
            => base.RenderRootComponent(componentId);

        public new Task RenderRootComponentAsync(int componentId)
            => base.RenderRootComponentAsync(componentId);

        public new Task RenderRootComponentAsync(int componentId, ParameterCollection parameters)
            => base.RenderRootComponentAsync(componentId, parameters);

        public new Task DispatchEventAsync(int componentId, int eventHandlerId, UIEventArgs args)
            => base.DispatchEventAsync(componentId, eventHandlerId, args);

        public T InstantiateComponent<T>() where T : IComponent
            => (T)InstantiateComponent(typeof(T));

        protected override bool HandleException(int componentId, IComponent component, Exception exception)
        {
            if (ShouldHandleExceptions)
            {
                HandledExceptions.Add((componentId, exception));
            }

            return ShouldHandleExceptions;
        }

        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
        {
            OnUpdateDisplay?.Invoke(renderBatch);

            var capturedBatch = new CapturedBatch();
            Batches.Add(capturedBatch);

            for (var i = 0; i < renderBatch.UpdatedComponents.Count; i++)
            {
                ref var renderTreeDiff = ref renderBatch.UpdatedComponents.Array[i];
                capturedBatch.AddDiff(renderTreeDiff);
            }

            // Clone other data, as underlying storage will get reused by later batches
            capturedBatch.ReferenceFrames = renderBatch.ReferenceFrames.ToArray();
            capturedBatch.DisposedComponentIDs = renderBatch.DisposedComponentIDs.ToList();

            // This renderer updates the UI synchronously, like the WebAssembly one.
            // To test async UI updates, subclass TestRenderer and override UpdateDisplayAsync.
            return Task.CompletedTask;
        }
    }
}
