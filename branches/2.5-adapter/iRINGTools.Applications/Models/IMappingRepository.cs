﻿using org.iringtools.mapping;

namespace org.iringtools.web.Models
{
  public interface IMappingRepository
  {
    Mapping GetMapping(string context, string endpoint);
    void UpdateMapping(Mapping mapping, string context, string endpoint);
  }
}