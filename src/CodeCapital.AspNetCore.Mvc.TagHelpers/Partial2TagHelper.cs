// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
#pragma warning disable 8618, 8625, 8600, 8765, 8604

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// Renders a partial view. Experimental update for child and nested partial views.
    /// </summary>
    [HtmlTargetElement("partial2", Attributes = "name", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class Partial2TagHelper : TagHelper
    {
        private const string ForAttributeName = "for";
        private const string ModelAttributeName = "model";
        private const string FallbackAttributeName = "fallback-name";
        private const string OptionalAttributeName = "optional";
        private const string RenderToAttributeName = "render-to";
        private object _model;
        private bool _hasModel;
        private bool _hasFor;
        private ModelExpression _for;

        private readonly ICompositeViewEngine _viewEngine;
        private readonly IViewBufferScope _viewBufferScope;

        /// <summary>
        /// Creates a new <see cref="Partial2TagHelper"/>.
        /// </summary>
        /// <param name="viewEngine">The <see cref="ICompositeViewEngine"/> used to locate the partial view.</param>
        /// <param name="viewBufferScope">The <see cref="IViewBufferScope"/>.</param>
        public Partial2TagHelper(
            ICompositeViewEngine viewEngine,
            IViewBufferScope viewBufferScope)
        {
            _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
            _viewBufferScope = viewBufferScope ?? throw new ArgumentNullException(nameof(viewBufferScope));
        }

        /// <summary>
        /// The name or path of the partial view that is rendered to the response.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// An expression to be evaluated against the current model. Cannot be used together with <see cref="Model"/>.
        /// </summary>
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For
        {
            get => _for;
            set
            {
                _for = value ?? throw new ArgumentNullException(nameof(value));
                _hasFor = true;
            }
        }

        /// <summary>
        /// The model to pass into the partial view. Cannot be used together with <see cref="For"/>.
        /// </summary>
        [HtmlAttributeName(ModelAttributeName)]
        public object Model
        {
            get => _model;
            set
            {
                _model = value;
                _hasModel = true;
            }
        }

        /// <summary>
        /// When optional, executing the tag helper will no-op if the view cannot be located. 
        /// Otherwise will throw stating the view could not be found.
        /// </summary>
        [HtmlAttributeName(OptionalAttributeName)]
        public bool Optional { get; set; }

        /// <summary>
        /// View to lookup if the view specified by <see cref="Name"/> cannot be located.
        /// </summary>
        [HtmlAttributeName(FallbackAttributeName)]
        public string FallbackName { get; set; }

        /// <summary>
        /// Render child to this ViewData kehy.
        /// </summary>
        [HtmlAttributeName(RenderToAttributeName)]
        public string RenderTo { get; set; } = "RenderChild";

        /// <summary>
        /// A <see cref="ViewDataDictionary"/> to pass into the partial view.
        /// </summary>
        public ViewDataDictionary ViewData { get; set; }

        /// <summary>
        /// Gets the <see cref="Rendering.ViewContext"/> of the executing view.
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Reset the TagName. We don't want `partial` to render.
            output.TagName = null;

            var result = FindView(Name);
            var viewSearchedLocations = result.SearchedLocations;
            var fallBackViewSearchedLocations = Enumerable.Empty<string>();

            if (!result.Success && !string.IsNullOrEmpty(FallbackName))
            {
                result = FindView(FallbackName);
                fallBackViewSearchedLocations = result.SearchedLocations;
            }

            if (!result.Success)
            {
                if (Optional)
                {
                    // Could not find the view or fallback view, but the partial is marked as optional.
                    return;
                }

                var locations = Environment.NewLine + string.Join(Environment.NewLine, viewSearchedLocations);
                var errorMessage = $"The partial view '{Name}' was not found. The following locations were searched:{locations}";

                if (!string.IsNullOrEmpty(FallbackName))
                {
                    locations = Environment.NewLine + string.Join(Environment.NewLine, result.SearchedLocations);
                    errorMessage += Environment.NewLine + $"The fallback partial view '{FallbackName}' was not found. The following locations were searched:{locations}";
                }

                throw new InvalidOperationException(errorMessage);
            }

            var model = ResolveModel();
            var viewBuffer = new ViewBuffer(_viewBufferScope, result.ViewName, ViewBuffer.PartialViewPageSize);
            using (var writer = new ViewBufferTextWriter(viewBuffer, Encoding.UTF8))
            {
                ViewContext.ViewData[RenderTo] = new HtmlString((await output.GetChildContentAsync()).GetContent() ?? "");
                await RenderPartialViewAsync(writer, model, result.View);
                output.Content.SetHtmlContent(viewBuffer);
            }
        }

        // Internal for testing
        internal object ResolveModel()
        {
            // 1. Disallow specifying values for both Model and For
            // 2. If a Model was assigned, use it even if it's null.
            // 3. For cannot have a null value. Use it if it was assigned to.
            // 4. Fall back to using the Model property on ViewContext.ViewData if none of the above conditions are met.

            if (_hasFor && _hasModel)
            {
                throw new InvalidOperationException(
                    $"Cannot use '{typeof(Partial2TagHelper).FullName}' with both '{ForAttributeName}' and '{ModelAttributeName}' attributes.");
            }

            if (_hasModel)
            {
                return Model;
            }

            if (_hasFor)
            {
                return For.Model;
            }

            // A value for Model or For was not specified, fallback to the ViewContext's ViewData model.
            return ViewContext.ViewData.Model;
        }

        private ViewEngineResult FindView(string partialName)
        {
            var viewEngineResult = _viewEngine.GetView(ViewContext.ExecutingFilePath, partialName, isMainPage: false);
            var getViewLocations = viewEngineResult.SearchedLocations;
            if (!viewEngineResult.Success)
            {
                viewEngineResult = _viewEngine.FindView(ViewContext, partialName, isMainPage: false);
            }

            if (!viewEngineResult.Success)
            {
                var searchedLocations = Enumerable.Concat(getViewLocations, viewEngineResult.SearchedLocations);
                return ViewEngineResult.NotFound(partialName, searchedLocations);
            }

            return viewEngineResult;
        }

        private async Task RenderPartialViewAsync(TextWriter writer, object model, IView view)
        {
            // Determine which ViewData we should use to construct a new ViewData
            var baseViewData = ViewData ?? ViewContext.ViewData;
            var newViewData = new ViewDataDictionary<object>(baseViewData, model);
            var partialViewContext = new ViewContext(ViewContext, view, newViewData, writer);

            if (For?.Name != null)
            {
                newViewData.TemplateInfo.HtmlFieldPrefix = newViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);
            }

            using (view as IDisposable)
            {
                await view.RenderAsync(partialViewContext);
            }
        }
    }

    public class ViewBuffer : IHtmlContentBuilder
    {
        public static readonly int PartialViewPageSize = 32;
        public static readonly int TagHelperPageSize = 32;
        public static readonly int ViewComponentPageSize = 32;
        public static readonly int ViewPageSize = 256;

        private readonly IViewBufferScope _bufferScope;
        private readonly string _name;
        private readonly int _pageSize;
        private ViewBufferPage _currentPage;         // Limits allocation if the ViewBuffer has only one page (frequent case).
        private List<ViewBufferPage> _multiplePages; // Allocated only if necessary

        /// <summary>
        /// Initializes a new instance of <see cref="ViewBuffer"/>.
        /// </summary>
        /// <param name="bufferScope">The <see cref="IViewBufferScope"/>.</param>
        /// <param name="name">A name to identify this instance.</param>
        /// <param name="pageSize">The size of buffer pages.</param>
        public ViewBuffer(IViewBufferScope bufferScope, string name, int pageSize)
        {
            if (bufferScope == null)
            {
                throw new ArgumentNullException(nameof(bufferScope));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            _bufferScope = bufferScope;
            _name = name;
            _pageSize = pageSize;
        }

        /// <summary>
        /// Get the <see cref="ViewBufferPage"/> count.
        /// </summary>
        public int Count
        {
            get
            {
                if (_multiplePages != null)
                {
                    return _multiplePages.Count;
                }
                if (_currentPage != null)
                {
                    return 1;
                }
                return 0;
            }
        }

        /// <summary>
        /// Gets a <see cref="ViewBufferPage"/>.
        /// </summary>
        public ViewBufferPage this[int index]
        {
            get
            {
                if (_multiplePages != null)
                {
                    return _multiplePages[index];
                }
                if (index == 0 && _currentPage != null)
                {
                    return _currentPage;
                }
                throw new IndexOutOfRangeException();
            }
        }

        /// <inheritdoc />
        // Very common trivial method; nudge it to inline https://github.com/aspnet/Mvc/pull/8339
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IHtmlContentBuilder Append(string unencoded)
        {
            if (unencoded != null)
            {
                // Text that needs encoding is the uncommon case in views, which is why it
                // creates a wrapper and pre-encoded text does not.
                AppendValue(new ViewBufferValue(new EncodingWrapper(unencoded)));
            }

            return this;
        }

        /// <inheritdoc />
        // Very common trivial method; nudge it to inline https://github.com/aspnet/Mvc/pull/8339
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IHtmlContentBuilder AppendHtml(IHtmlContent content)
        {
            if (content != null)
            {
                AppendValue(new ViewBufferValue(content));
            }

            return this;
        }

        /// <inheritdoc />
        // Very common trivial method; nudge it to inline https://github.com/aspnet/Mvc/pull/8339
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IHtmlContentBuilder AppendHtml(string encoded)
        {
            if (encoded != null)
            {
                AppendValue(new ViewBufferValue(encoded));
            }

            return this;
        }

        // Very common trivial method; nudge it to inline https://github.com/aspnet/Mvc/pull/8339
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendValue(ViewBufferValue value)
        {
            var page = GetCurrentPage();
            page.Append(value);
        }

        // Very common trivial method; nudge it to inline https://github.com/aspnet/Mvc/pull/8339
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ViewBufferPage GetCurrentPage()
        {
            var currentPage = _currentPage;
            if (currentPage == null || currentPage.IsFull)
            {
                // Uncommon slow-path
                return AppendNewPage();
            }

            return currentPage;
        }

        // Slow path for above, don't inline
        [MethodImpl(MethodImplOptions.NoInlining)]
        private ViewBufferPage AppendNewPage()
        {
            AddPage(new ViewBufferPage(_bufferScope.GetPage(_pageSize)));
            return _currentPage;
        }

        private void AddPage(ViewBufferPage page)
        {
            if (_multiplePages != null)
            {
                _multiplePages.Add(page);
            }
            else if (_currentPage != null)
            {
                _multiplePages = new List<ViewBufferPage>(2);
                _multiplePages.Add(_currentPage);
                _multiplePages.Add(page);
            }

            _currentPage = page;
        }

        /// <inheritdoc />
        public IHtmlContentBuilder Clear()
        {
            _multiplePages = null;
            _currentPage = null;
            return this;
        }

        /// <inheritdoc />
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            for (var i = 0; i < Count; i++)
            {
                var page = this[i];
                for (var j = 0; j < page.Count; j++)
                {
                    var value = page.Buffer[j];

                    if (value.Value is string valueAsString)
                    {
                        writer.Write(valueAsString);
                        continue;
                    }

                    if (value.Value is IHtmlContent valueAsHtmlContent)
                    {
                        valueAsHtmlContent.WriteTo(writer, encoder);
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Writes the buffered content to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/>.</param>
        /// <param name="encoder">The <see cref="HtmlEncoder"/>.</param>
        /// <returns>A <see cref="Task"/> which will complete once content has been written.</returns>
        public async Task WriteToAsync(TextWriter writer, HtmlEncoder encoder)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            for (var i = 0; i < Count; i++)
            {
                var page = this[i];
                for (var j = 0; j < page.Count; j++)
                {
                    var value = page.Buffer[j];

                    if (value.Value is string valueAsString)
                    {
                        await writer.WriteAsync(valueAsString);
                        continue;
                    }

                    if (value.Value is ViewBuffer valueAsViewBuffer)
                    {
                        await valueAsViewBuffer.WriteToAsync(writer, encoder);
                        continue;
                    }

                    if (value.Value is IHtmlContent valueAsHtmlContent)
                    {
                        valueAsHtmlContent.WriteTo(writer, encoder);
                        await writer.FlushAsync();
                        continue;
                    }
                }
            }
        }

        private string DebuggerToString() => _name;

        public void CopyTo(IHtmlContentBuilder destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            for (var i = 0; i < Count; i++)
            {
                var page = this[i];
                for (var j = 0; j < page.Count; j++)
                {
                    var value = page.Buffer[j];

                    string valueAsString;
                    IHtmlContentContainer valueAsContainer;
                    if ((valueAsString = value.Value as string) != null)
                    {
                        destination.AppendHtml(valueAsString);
                    }
                    else if ((valueAsContainer = value.Value as IHtmlContentContainer) != null)
                    {
                        valueAsContainer.CopyTo(destination);
                    }
                    else
                    {
                        destination.AppendHtml((IHtmlContent)value.Value);
                    }
                }
            }
        }

        public void MoveTo(IHtmlContentBuilder destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            // Perf: We have an efficient implementation when the destination is another view buffer,
            // we can just insert our pages as-is.
            if (destination is ViewBuffer other)
            {
                MoveTo(other);
                return;
            }

            for (var i = 0; i < Count; i++)
            {
                var page = this[i];
                for (var j = 0; j < page.Count; j++)
                {
                    var value = page.Buffer[j];

                    string valueAsString;
                    IHtmlContentContainer valueAsContainer;
                    if ((valueAsString = value.Value as string) != null)
                    {
                        destination.AppendHtml(valueAsString);
                    }
                    else if ((valueAsContainer = value.Value as IHtmlContentContainer) != null)
                    {
                        valueAsContainer.MoveTo(destination);
                    }
                    else
                    {
                        destination.AppendHtml((IHtmlContent)value.Value);
                    }
                }
            }

            for (var i = 0; i < Count; i++)
            {
                var page = this[i];
                Array.Clear(page.Buffer, 0, page.Count);
                _bufferScope.ReturnSegment(page.Buffer);
            }

            Clear();
        }

        private void MoveTo(ViewBuffer destination)
        {
            for (var i = 0; i < Count; i++)
            {
                var page = this[i];

                var destinationPage = destination.Count == 0 ? null : destination[destination.Count - 1];

                // If the source page is less or equal to than half full, let's copy it's content to the destination
                // page if possible.
                var isLessThanHalfFull = 2 * page.Count <= page.Capacity;
                if (isLessThanHalfFull &&
                    destinationPage != null &&
                    destinationPage.Capacity - destinationPage.Count >= page.Count)
                {
                    // We have room, let's copy the items.
                    Array.Copy(
                        sourceArray: page.Buffer,
                        sourceIndex: 0,
                        destinationArray: destinationPage.Buffer,
                        destinationIndex: destinationPage.Count,
                        length: page.Count);

                    destinationPage.Count += page.Count;

                    // Now we can return the source page, and it can be reused in the scope of this request.
                    Array.Clear(page.Buffer, 0, page.Count);
                    _bufferScope.ReturnSegment(page.Buffer);

                }
                else
                {
                    // Otherwise, let's just add the source page to the other buffer.
                    destination.AddPage(page);
                }

            }

            Clear();
        }

        private class EncodingWrapper : IHtmlContent
        {
            private readonly string _unencoded;

            public EncodingWrapper(string unencoded)
            {
                _unencoded = unencoded;
            }

            public void WriteTo(TextWriter writer, HtmlEncoder encoder)
            {
                encoder.Encode(writer, _unencoded);
            }
        }
    }

    public class ViewBufferPage
    {
        public ViewBufferPage(ViewBufferValue[] buffer)
        {
            Buffer = buffer;
        }

        public ViewBufferValue[] Buffer { get; }

        public int Capacity => Buffer.Length;

        public int Count { get; set; }

        public bool IsFull => Count == Capacity;

        // Very common trivial method; nudge it to inline https://github.com/aspnet/Mvc/pull/8339
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(ViewBufferValue value) => Buffer[Count++] = value;
    }

    public class ViewBufferTextWriter : TextWriter
    {
        private readonly TextWriter _inner;
        private readonly HtmlEncoder _htmlEncoder;

        /// <summary>
        /// Creates a new instance of <see cref="ViewBufferTextWriter"/>.
        /// </summary>
        /// <param name="buffer">The <see cref="ViewBuffer"/> for buffered output.</param>
        /// <param name="encoding">The <see cref="System.Text.Encoding"/>.</param>
        public ViewBufferTextWriter(ViewBuffer buffer, Encoding encoding)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            Buffer = buffer;
            Encoding = encoding;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ViewBufferTextWriter"/>.
        /// </summary>
        /// <param name="buffer">The <see cref="ViewBuffer"/> for buffered output.</param>
        /// <param name="encoding">The <see cref="System.Text.Encoding"/>.</param>
        /// <param name="htmlEncoder">The HTML encoder.</param>
        /// <param name="inner">
        /// The inner <see cref="TextWriter"/> to write output to when this instance is no longer buffering.
        /// </param>
        public ViewBufferTextWriter(ViewBuffer buffer, Encoding encoding, HtmlEncoder htmlEncoder, TextWriter inner)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            if (htmlEncoder == null)
            {
                throw new ArgumentNullException(nameof(htmlEncoder));
            }

            if (inner == null)
            {
                throw new ArgumentNullException(nameof(inner));
            }

            Buffer = buffer;
            Encoding = encoding;
            _htmlEncoder = htmlEncoder;
            _inner = inner;
        }

        /// <inheritdoc />
        public override Encoding Encoding { get; }

        /// <summary>
        /// Gets the <see cref="ViewBuffer"/>.
        /// </summary>
        public ViewBuffer Buffer { get; }

        /// <summary>
        /// Gets a value that indiciates if <see cref="Flush"/> or <see cref="FlushAsync" /> was invoked.
        /// </summary>
        public bool Flushed { get; private set; }

        /// <inheritdoc />
        public override void Write(char value)
        {
            Buffer.AppendHtml(value.ToString());
        }

        /// <inheritdoc />
        public override void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0 || index >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0 || (buffer.Length - index < count))
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            Buffer.AppendHtml(new string(buffer, index, count));
        }

        /// <inheritdoc />
        public override void Write(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            Buffer.AppendHtml(value);
        }

        /// <inheritdoc />
        public override void Write(object value)
        {
            if (value == null)
            {
                return;
            }

            if (value is IHtmlContentContainer container)
            {
                Write(container);
            }
            else if (value is IHtmlContent htmlContent)
            {
                Write(htmlContent);
            }
            else
            {
                Write(value.ToString());
            }
        }

        /// <summary>
        /// Writes an <see cref="IHtmlContent"/> value.
        /// </summary>
        /// <param name="value">The <see cref="IHtmlContent"/> value.</param>
        public void Write(IHtmlContent value)
        {
            if (value == null)
            {
                return;
            }

            Buffer.AppendHtml(value);
        }

        /// <summary>
        /// Writes an <see cref="IHtmlContentContainer"/> value.
        /// </summary>
        /// <param name="value">The <see cref="IHtmlContentContainer"/> value.</param>
        public void Write(IHtmlContentContainer value)
        {
            if (value == null)
            {
                return;
            }

            value.MoveTo(Buffer);
        }

        /// <inheritdoc />
        public override void WriteLine(object value)
        {
            if (value == null)
            {
                return;
            }

            if (value is IHtmlContentContainer container)
            {
                Write(container);
                Write(NewLine);
            }
            else if (value is IHtmlContent htmlContent)
            {
                Write(htmlContent);
                Write(NewLine);
            }
            else
            {
                Write(value.ToString());
                Write(NewLine);
            }
        }

        /// <inheritdoc />
        public override Task WriteAsync(char value)
        {
            Buffer.AppendHtml(value.ToString());
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0 || (buffer.Length - index < count))
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            Buffer.AppendHtml(new string(buffer, index, count));
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task WriteAsync(string value)
        {
            Buffer.AppendHtml(value);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override void WriteLine()
        {
            Buffer.AppendHtml(NewLine);
        }

        /// <inheritdoc />
        public override void WriteLine(string value)
        {
            Buffer.AppendHtml(value);
            Buffer.AppendHtml(NewLine);
        }

        /// <inheritdoc />
        public override Task WriteLineAsync(char value)
        {
            Buffer.AppendHtml(value.ToString());
            Buffer.AppendHtml(NewLine);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task WriteLineAsync(char[] value, int start, int offset)
        {
            Buffer.AppendHtml(new string(value, start, offset));
            Buffer.AppendHtml(NewLine);
            return Task.CompletedTask;

        }

        /// <inheritdoc />
        public override Task WriteLineAsync(string value)
        {
            Buffer.AppendHtml(value);
            Buffer.AppendHtml(NewLine);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task WriteLineAsync()
        {
            Buffer.AppendHtml(NewLine);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Copies the buffered content to the unbuffered writer and invokes flush on it.
        /// </summary>
        public override void Flush()
        {
            if (_inner == null || _inner is ViewBufferTextWriter)
            {
                return;
            }

            Flushed = true;

            Buffer.WriteTo(_inner, _htmlEncoder);
            Buffer.Clear();

            _inner.Flush();
        }

        /// <summary>
        /// Copies the buffered content to the unbuffered writer and invokes flush on it.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous copy and flush operations.</returns>
        public override async Task FlushAsync()
        {
            if (_inner == null || _inner is ViewBufferTextWriter)
            {
                return;
            }

            Flushed = true;

            await Buffer.WriteToAsync(_inner, _htmlEncoder);
            Buffer.Clear();

            await _inner.FlushAsync();
        }
    }
}
