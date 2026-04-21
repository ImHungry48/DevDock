using System;
using System.Collections.Generic;
using System.Linq;
using DevDock.Data;
using DevDock.Models;

/// <summary>
/// Handles CRUD operations for saved code snippets.
/// This service keeps the persistence logic out of the viewmodels.
/// </summary>
namespace DevDock.Services
{
    public class CodeSnippetStorageService
    {
        /// <summary>
        /// Returns all snippets sorted alphabetically by title so the UI stays predictable.
        /// </summary>
        public List<CodeSnippet> GetAllSnippets()
        {
            using var db = new AppDbContext();
            return db.CodeSnippets
                .OrderBy(s => s.Title)
                .ToList();
        }

        /// <summary>
        /// Saves a snippet by either inserting a new row or updating an existing one.
        /// </summary>
        public void SaveSnippet(CodeSnippet snippet)
        {
            using var db = new AppDbContext();

            var existingSnippet = db.CodeSnippets
                .FirstOrDefault(s => s.Id == snippet.Id);

            if (existingSnippet == null)
            {
                db.CodeSnippets.Add(new CodeSnippet
                {
                    Id = snippet.Id,
                    Title = snippet.Title,
                    Content = snippet.Content
                });
            }
            else
            {
                existingSnippet.Title = snippet.Title;
                existingSnippet.Content = snippet.Content;
            }

            db.SaveChanges();
        }

        /// <summary>
        /// Deletes the matching snippet if it exists.
        /// </summary>
        public void DeleteSnippet(CodeSnippet snippet)
        {
            using var db = new AppDbContext();

            var existingSnippet = db.CodeSnippets
                .FirstOrDefault(s => s.Id == snippet.Id);

            if (existingSnippet != null)
            {
                db.CodeSnippets.Remove(existingSnippet);
                db.SaveChanges();
            }
        }
    }
}