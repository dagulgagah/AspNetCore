// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    /// <summary>
    /// Extensions for rendering components.
    /// </summary>
    public static class HtmlHelperComponentExtensions
    {
        /// <summary>
        /// Renders the <typeparamref name="TComponent"/> <see cref="IComponent"/>.
        /// </summary>
        /// <param name="htmlHelper">The <see cref="IHtmlHelper"/>.</param>
        /// <returns>The HTML produced by the rendered <typeparamref name="TComponent"/>.</returns>
        public static Task<IHtmlContent> RenderComponentAsync<TComponent>(this IHtmlHelper htmlHelper) where TComponent : IComponent
        {
            return htmlHelper.RenderComponentAsync<TComponent>(null);
        }

        /// <summary>
        /// Renders the <typeparamref name="TComponent"/> <see cref="IComponent"/>.
        /// </summary>
        /// <param name="htmlHelper">The <see cref="IHtmlHelper"/>.</param>
        /// <param name="parameters">An <see cref="object"/> containing the parameters to pass
        /// to the component.</param>
        /// <returns>The HTML produced by the rendered <typeparamref name="TComponent"/>.</returns>
        public static Task<IHtmlContent> RenderComponentAsync<TComponent>(
            this IHtmlHelper htmlHelper,
            object parameters) where TComponent : IComponent
        {
            return htmlHelper.RenderComponentAsync<TComponent>(HtmlHelper.ObjectToDictionary(parameters));
        }

        /// <summary>
        /// Renders the <typeparamref name="TComponent"/> <see cref="IComponent"/>.
        /// </summary>
        /// <param name="htmlHelper">The <see cref="IHtmlHelper"/>.</param>
        /// <param name="parameters">An <see cref="IDictionary{TKey, TValue}"/> containing the parameters to pass
        /// to the component.</param>
        /// <returns>The HTML produced by the rendered <typeparamref name="TComponent"/>.</returns>
        public static async Task<IHtmlContent> RenderComponentAsync<TComponent>(
            this IHtmlHelper htmlHelper,
            IDictionary<string,object> parameters) where TComponent : IComponent
        {
            var serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
            var encoder = serviceProvider.GetRequiredService<HtmlEncoder>();
            var htmlRenderer = new HtmlRenderer(serviceProvider, encoder.Encode);
            var result = await htmlRenderer.RenderComponentAsync<TComponent>(
                parameters == null ? ParameterCollection.Empty : ParameterCollection.FromDictionary(parameters));

            return new ComponentHtmlContent(result);
        }

        private class ComponentHtmlContent : IHtmlContent
        {
            private readonly IEnumerable<string> _componentResult;

            public ComponentHtmlContent(IEnumerable<string> componentResult)
            {
                _componentResult = componentResult;
            }

            public void WriteTo(TextWriter writer, HtmlEncoder encoder)
            {
                foreach (var element in _componentResult)
                {
                    writer.Write(element);
                }
            }
        }
    }
}
