using System;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;


namespace SixDegrees.Mvc
{
    //public class GenericBinderResolver : DefaultModelBinder
    //{
    //    private static readonly Type BinderType = typeof(IModelBinder<>);

    //    public override object BindModel(ControllerContext controllerContext,
    //                                     ModelBindingContext bindingContext)
    //    {
    //        Type genericBinderType = BinderType.MakeGenericType(bindingContext.ModelType);

    //        var binder = ObjectFactory.TryGetInstance(genericBinderType) as IModelBinder;
    //        if (null != binder) return binder.BindModel(controllerContext, bindingContext);

    //        return base.BindModel(controllerContext, bindingContext);
    //    }
    //}

    //public interface IModelBinder<T> : IModelBinder
    //{

    //}

    //public abstract class ModelBinder<T> : IModelBinder<T>
    //{
    //    public abstract T BindModel(ControllerContext controllerContext,
    //                                   ModelBindingContext bindingContext);

    //    object IModelBinder.BindModel(ControllerContext controllerContext,
    //                                  ModelBindingContext bindingContext)
    //    {
    //        return BindModel(controllerContext, bindingContext);
    //    }
    //}

    //public class CsvModelBinder : ModelBinder<string[]>
    //{
    //    public override string[] BindModel(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
    //    {
    //        return bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue.Split(
    //            new[] { ',' },
    //            StringSplitOptions.RemoveEmptyEntries);
    //    }
    //}


	public class XmlResult : ActionResult
	{
		private readonly XDocument _document;
		private readonly string _etag;

		public XmlResult(XDocument document, string etag)
		{
			_document = document;
			_etag = etag;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			if (_etag != null)
				context.HttpContext.Response.AddHeader("ETag", _etag);

			context.HttpContext.Response.ContentType = "text/xml";

			using (var xmlWriter = XmlWriter.Create(context.HttpContext.Response.OutputStream))
			{
				_document.WriteTo(xmlWriter);
				xmlWriter.Flush();
			}
		}
	}
}