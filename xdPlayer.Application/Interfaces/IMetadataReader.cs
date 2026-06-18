using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Application.Interfaces;

public interface IMetadataReader
{
    Track ReadMetadata(string filePath);
}