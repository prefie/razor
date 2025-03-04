﻿using System;
using BadNews.ModelBuilders.News;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BadNews.Controllers
{
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client, VaryByHeader = "Cookie")]
    public class NewsController : Controller
    {
        private readonly INewsModelBuilder newsModelBuilder;
        private readonly IMemoryCache memoryCache;

        public NewsController(INewsModelBuilder newsModelBuilder, IMemoryCache memoryCache)
        {
            this.newsModelBuilder = newsModelBuilder;
            this.memoryCache = memoryCache;
        }

        public IActionResult Index(int? year, int pageIndex = 0)
        {
            return View(newsModelBuilder.BuildIndexModel(pageIndex, true, year));
        }
        
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult FullArticle(Guid id)
        {
            var cacheKey = $"{nameof(NewsController)}-{nameof(FullArticle)}-{id}";
            if (!memoryCache.TryGetValue(cacheKey, out var model))
            {
                model = newsModelBuilder.BuildFullArticleModel(id);
                if (model != null)
                {
                    memoryCache.Set(cacheKey, model, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromSeconds(30)
                    });
                }
            }
            
            if (model == null)
                return NotFound();
            return View(model);
        }
    }
}