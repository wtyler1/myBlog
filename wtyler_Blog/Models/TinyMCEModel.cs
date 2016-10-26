using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace wtyler_Blog.Models {

    public class TinyMCEModel {

        [AllowHtml]
        [UIHint("tinymce_full_compressed")]
        public string Content { get; set; }

    }
}