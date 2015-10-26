using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace WebDeploy.Web.Extend
{
    enum PagerEntityType { Prev = 1, Index, First, Last, Next, To }

    [Serializable]
    class PagerEntity
    {
        private PagerEntityType m_Type;
        private int m_Index;
        private bool m_Enable;
        private bool m_Selected;

        public PagerEntity(PagerEntityType type, int index, bool selected, bool enable)
        {
            m_Type = type;
            m_Index = index;
            m_Enable = enable;
            m_Selected = selected;
        }

        public PagerEntityType Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        public int Index
        {
            get { return m_Index; }
            set { m_Index = value; }
        }

        public bool Enable
        {
            get { return m_Enable; }
            set { m_Enable = value; }
        }

        public bool Selected
        {
            get { return m_Selected; }
            set { m_Selected = value; }
        }
    }

    interface IPagerTemplate
    {
        string IndexSelect { get; }
        string IndexUnSelect { get; }

        string PrevEnable { get; }
        string PrevUnEnable { get; }

        string NextEnable { get; }
        string NextUnEnable { get; }

        string To { get; }

        string First { get; }

        string Last { get; }

        string Frame { get; }
    }

    class CommonPagerTemplate : IPagerTemplate
    {
        public string IndexSelect
        {
            get { return "<li class=\"pager-selected\">{Index}</li>"; }
        }
        public string IndexUnSelect
        {
            get { return "<li><a href=\"{Uri}\">{Index}</a></li>"; }
        }

        public string PrevEnable
        {
            get { return "<li class=\"pager-prev\"><a href=\"{Uri}\">{Prev}</a></li>"; }
        }
        public string PrevUnEnable
        {
            get { return "<li class=\"pager-prev\">{Prev}</li>"; }
        }

        public string NextEnable
        {
            get { return "<li class=\"pager-next\"><a href=\"{Uri}\">{Next}</a></li>"; }
        }
        public string NextUnEnable
        {
            get { return "<li class=\"pager-next\">{Next}</li>"; }
        }

        public string To
        {
            get { return "<li class=\"pager-to\"><input id=\"pager-jump\" type=\"text\" class=\"pager-text\" /><span class=\"btn btn-normal\"><span><button type=\"button\" onclick=\"location.href = '{Uri}'.replace('{Index}', document.getElementById('pager-jump').value.replace(/^\\s+|\\s+$/g, ''))\">{Go}</button></span></span></li>"; }
        }

        public string First
        {
            get { return "<li><a href=\"{Uri}\">{First}</a></li><li class=\"pager-more\">...</li>"; }
        }

        public string Last
        {
            get { return "<li class=\"pager-more\">...</li><li><a href=\"{Uri}\">{Last}</a></li>"; }
        }
        public string Frame
        {
            get { return "<ul class=\"pager\">{Con}</ul>"; }
        }
    }

    class ViewPager
    {
        protected IPagerTemplate m_Tpl = null;
        protected int m_Index = 0;
        protected int m_Total = 0;
        protected int m_Record = 0;
        protected int m_Count = 0;
        protected string m_Uri = string.Empty;
        protected string m_Where = string.Empty;

        protected string m_FirstName = "{Index}";
        protected string m_LastName = "{Index}";
        protected string m_PrevName = "前一页";
        protected string m_NextName = "下一页";
        protected string m_GoName = "确定";

        public int Index
        {
            get { return m_Index; }
        }

        public int Total
        {
            get { return m_Total; }
        }

        public int Record
        {
            get { return m_Record; }
        }

        public int Count
        {
            get { return m_Count; }
        }

        public string FirstName
        {
            get { return m_FirstName; }
            set { m_FirstName = value; }
        }

        public string LastName
        {
            get { return m_LastName; }
            set { m_LastName = value; }
        }

        public string PrevName
        {
            get { return m_PrevName; }
            set { m_PrevName = value; }
        }

        public string NextName
        {
            get { return m_NextName; }
            set { m_NextName = value; }
        }

        public string GoName
        {
            get { return m_GoName; }
            set { m_GoName = value; }
        }

        public ViewPager(IPagerTemplate tpl, string uri, int record, int total)
            : this(tpl, uri, 1, record, total)
        {
        }

        public ViewPager(IPagerTemplate tpl, string uri, int index, int record, int total)
            : this(tpl, uri, index, record, total, null)
        {
        }

        public ViewPager(IPagerTemplate tpl, string uri, int index, int record, int total, IDictionary<string, string> where)
        {
            m_Tpl = tpl;
            m_Uri = uri;
            m_Index = index;
            m_Record = record;
            m_Total = total;

            if (where != null)
            {
                InitWhere(where);
            }
        }

        private void InitWhere(IDictionary<string, string> where)
        {
            var ret = string.Empty;

            if (where == null)
            {
                return;
            }

            var jump = string.Empty;

            var m = System.Text.RegularExpressions.Regex.Match(m_Uri, @"[^=?&]+(?==\{Index\})");

            if (m.Success)
            {
                jump = m.Value.ToLower();
            }

            foreach (var property in where.Keys)
            {
                var value = where[property];
                var name = property;

                if (name.ToLower() == jump)
                {
                    continue;
                }

                ret += "&" + name + "=" + System.Web.HttpContext.Current.Server.UrlEncode(value);
            }

            m_Where = ret;
        }

        public void To(int index)
        {
            m_Index = index;
        }

        protected IList<PagerEntity> Builder()
        {
            IList<PagerEntity> list = new List<PagerEntity>();

            if (m_Record == 0)
            {
                return list;
            }

            int count = Convert.ToInt32(Math.Ceiling((double)m_Total / (double)m_Record));

            if (count <= 1)
            {
                return list;
            }

            if (m_Index > 1)
            {
                list.Add(new PagerEntity(PagerEntityType.Prev, m_Index - 1, false, true));
            }
            else
            {
                list.Add(new PagerEntity(PagerEntityType.Prev, 0, false, false));
            }

            if (count > 12)
            {
                if (m_Index < 8)
                {
                    for (int i = 1; i <= Math.Min(count, 9); ++i)
                    {
                        if (m_Index == i)
                        {
                            list.Add(new PagerEntity(PagerEntityType.Index, i, true, true));
                        }
                        else
                        {
                            list.Add(new PagerEntity(PagerEntityType.Index, i, false, true));
                        }
                    }

                    list.Add(new PagerEntity(PagerEntityType.Last, count, false, true));

                }
                else if (m_Index + 8 > count)
                {
                    list.Add(new PagerEntity(PagerEntityType.First, 1, false, true));

                    for (int i = Math.Max(count - 9, 1); i <= count; ++i)
                    {
                        if (m_Index == i)
                        {
                            list.Add(new PagerEntity(PagerEntityType.Index, i, true, true));
                        }
                        else
                        {
                            list.Add(new PagerEntity(PagerEntityType.Index, i, false, true));
                        }
                    }
                }
                else
                {

                    list.Add(new PagerEntity(PagerEntityType.First, 1, false, true));

                    for (int i = m_Index - 4; i < m_Index; ++i)
                    {
                        list.Add(new PagerEntity(PagerEntityType.Index, i, false, true));
                    }

                    list.Add(new PagerEntity(PagerEntityType.Index, m_Index, true, true));

                    for (int i = m_Index + 1; i <= m_Index + 4; ++i)
                    {
                        list.Add(new PagerEntity(PagerEntityType.Index, i, false, true));
                    }

                    list.Add(new PagerEntity(PagerEntityType.Last, count, false, true));

                }

            }
            else
            {
                for (int i = 1; i <= count; ++i)
                {
                    if (m_Index == i)
                    {
                        list.Add(new PagerEntity(PagerEntityType.Index, i, true, true));
                    }
                    else
                    {
                        list.Add(new PagerEntity(PagerEntityType.Index, i, false, true));
                    }
                }
            }

            if (m_Index < count)
            {
                list.Add(new PagerEntity(PagerEntityType.Next, m_Index + 1, false, true));
            }
            else
            {
                list.Add(new PagerEntity(PagerEntityType.Next, 0, false, false));
            }

            if (count > 12)
            {
                list.Add(new PagerEntity(PagerEntityType.To, 0, false, true));
            }

            return list;
        }

        public string Drow()
        {
            IList<PagerEntity> list = Builder();

            StringBuilder ret = new StringBuilder();

            for (int i = 0; i < list.Count; ++i)
            {
                PagerEntity entity = list[i];

                string tpl = string.Empty;

                if (entity.Type == PagerEntityType.Index)
                {
                    if (entity.Selected)
                    {
                        tpl = m_Tpl.IndexSelect;
                    }
                    else
                    {
                        tpl = m_Tpl.IndexUnSelect;
                    }
                }
                else if (entity.Type == PagerEntityType.Prev)
                {
                    if (entity.Enable)
                    {
                        tpl = m_Tpl.PrevEnable;
                    }
                    else
                    {
                        tpl = m_Tpl.PrevUnEnable;
                    }
                }
                else if (entity.Type == PagerEntityType.Next)
                {
                    if (entity.Enable)
                    {
                        tpl = m_Tpl.NextEnable;
                    }
                    else
                    {
                        tpl = m_Tpl.NextUnEnable;
                    }
                }
                else if (entity.Type == PagerEntityType.To)
                {
                    tpl = m_Tpl.To;
                }
                else if (entity.Type == PagerEntityType.First)
                {
                    tpl = m_Tpl.First;
                }
                else if (entity.Type == PagerEntityType.Last)
                {
                    tpl = m_Tpl.Last;
                }

                tpl = tpl.Replace("{Uri}", m_Uri.Replace("{Index}", "{Index}" + m_Where)).Replace("{First}", m_FirstName).Replace("{Last}", m_LastName).Replace("{Next}", m_NextName).Replace("{Prev}", m_PrevName).Replace("{Go}", m_GoName);

                if (entity.Type != PagerEntityType.To)
                {
                    tpl = tpl.Replace("{Index}", entity.Index.ToString());
                }

                ret.Append(tpl);
            }

            return m_Tpl.Frame.Replace("{Con}", ret.ToString());
        }
    }

    public static class PagerHtmlHelper
    {
        public static MvcHtmlString Pager(this HtmlHelper helper, string uri)
        {
            var index = Convert.ToInt32(helper.ViewData["__pager_index"]);
            var limit = Convert.ToInt32(helper.ViewData["__pager_limit"]);
            var total = Convert.ToInt32(helper.ViewData["__pager_total"]);
            var where = helper.ViewData["__pager_where"];

            var tpl = new CommonPagerTemplate();

            IDictionary<string, string> iwhere = null;

            if (where != null && where.GetType().Name.StartsWith("<>f__AnonymousType"))
            {
                iwhere = LinqWhere(where);
            }
            else if (System.Web.HttpContext.Current != null)
            {
                iwhere = QueryStringWhere(System.Web.HttpContext.Current.Request.QueryString);
            }
            else
            {
                iwhere = new Dictionary<string, string>();
            }

            var pager = new ViewPager(tpl, uri, index, limit, total, iwhere);

            return MvcHtmlString.Create(pager.Drow());
        }

        private static IDictionary<string, string> LinqWhere(object where)
        {
            var ret = new Dictionary<string, string>();

            foreach (var property in where.GetType().GetProperties())
            {
                var value = Convert.ToString(property.GetValue(where, null));
                var name = property.Name;

                ret[name] = value;
            }

            return ret;
        }

        private static IDictionary<string, string> QueryStringWhere(System.Collections.Specialized.NameValueCollection querystring)
        {
            var ret = new Dictionary<string, string>();

            foreach (var key in querystring.AllKeys)
            {
                var value = Convert.ToString(querystring[key]);
                var name = key;

                ret[name] = value;
            }

            return ret;
        }

        private static string WriteWhere(IDictionary<string, bool> dumps, IDictionary<string, string> where, string tpl)
        {
            var ret = new StringBuilder();

            foreach (var property in where.Keys)
            {
                var value = where[property];
                var name = property;

                if (dumps.Count > 0 && !dumps.ContainsKey(name))
                {
                    continue;
                }

                ret.Append(tpl.Replace("{name}", name).Replace("{value}", value));
            }

            return ret.ToString();
        }

        public static MvcHtmlString PagerBindWhere(this HtmlHelper helper)
        {
            return PagerBindWhere(helper, string.Empty);
        }

        public static MvcHtmlString PagerBindWhere(this HtmlHelper helper, string dump)
        {
            return PagerBindWhere(helper, dump, "$('[name=\"{name}\"]').val('{value}');");
        }

        public static MvcHtmlString PagerBindWhere(this HtmlHelper helper, string dump, string tpl)
        {
            dump = dump.Trim().Trim(',').Trim();

            var dumps = new Dictionary<string, bool>();

            if (dump != string.Empty)
            {
                var sdump = dump.Split(',');

                foreach (var i in sdump)
                {
                    dumps[i] = true;
                }
            }

            var where = helper.ViewData["__pager_where"];

            IDictionary<string, string> iwhere = null;

            if (where != null && where.GetType().Name.StartsWith("<>f__AnonymousType"))
            {
                iwhere = LinqWhere(where);
            }
            else if (System.Web.HttpContext.Current != null)
            {
                iwhere = QueryStringWhere(System.Web.HttpContext.Current.Request.QueryString);
            }
            else
            {
                iwhere = new Dictionary<string, string>();
            }

            var ret = WriteWhere(dumps, iwhere, tpl);

            return MvcHtmlString.Create(ret);
        }

        public static void Generate(ViewDataDictionary data, int index, int limit, int total)
        {
            Generate(data, index, limit, total, string.Empty);
        }

        public static void Generate(ViewDataDictionary data, int index, int limit, int total, object where)
        {
            data["__pager_index"] = index;
            data["__pager_limit"] = limit;
            data["__pager_total"] = total;
            data["__pager_where"] = where;
        }
    }

}