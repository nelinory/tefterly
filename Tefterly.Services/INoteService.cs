﻿using System;
using System.Collections.Generic;
using Tefterly.Business.Models;

namespace Tefterly.Services
{
    public interface INoteService
    {
        IList<Notebook> GetAllNotebookCategories();
        int GetCategoryCount(Guid category);
        IList<Note> GetNotes(Guid category);
        Note GetNote(Guid id);
    }
}
