# AI Book Author Pro

**The Ultimate AI-Powered Book Writing Application for Windows**

[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-blue)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## Overview

AI Book Author Pro is a revolutionary Windows desktop application designed for authors who want to leverage AI to write novels, non-fiction books, and other long-form content. With deep integration of Claude and GPT models, intelligent context management, and a beautiful Material Design interface, you can write your masterpiece faster than ever.

## Features

### 📖 Project Management
- Create and manage book projects with rich metadata
- Automatic backup and version history
- Project templates for novels, non-fiction, memoirs
- Custom `.abpro` file format (ZIP-based)

### ✍️ AI-Powered Writing
- **Claude & OpenAI Integration** - Use the best AI models for writing
- **Context-Aware Generation** - AI understands your characters, locations, and plot
- **Streaming Output** - Watch your story unfold in real-time
- **Multiple Generation Modes** - Generate, continue, expand, or rewrite
- **Smart Token Management** - Optimize context for best results

### 👥 Character Management
- Detailed character profiles with backstory and relationships
- AI-generated character descriptions and dialogue
- Character relationship mapping
- Track character appearances across chapters

### 🗺️ Location Management
- Rich location descriptions with sensory details
- AI-generated atmosphere and settings
- Location type categorization

### 📋 Outline Tools
- Hierarchical outline structure (Acts, Parts, Chapters, Scenes)
- AI-generated outlines from your premise
- Drag-and-drop organization
- Link outline items to chapters

### 📤 Export Options
- **Word (.docx)** - Professional manuscript format
- **PDF** - Print-ready output
- **EPUB** - eBook distribution
- **Markdown** - Plain text with formatting
- **HTML** - Web publishing

### ⌨️ Keyboard Shortcuts
| Action | Shortcut |
|--------|----------|
| New Project | Ctrl+N |
| Open Project | Ctrl+O |
| Save | Ctrl+S |
| Export | Ctrl+Shift+E |
| Dashboard | Ctrl+1 |
| Editor | Ctrl+2 |
| Characters | Ctrl+3 |
| Locations | Ctrl+4 |
| Outline | Ctrl+5 |
| Settings | Ctrl+, |
| Toggle Navigation | Ctrl+B |

## Architecture

```
AIBookAuthorPro/
├── src/
│   ├── AIBookAuthorPro.Core/        # Domain models, interfaces, validation
│   ├── AIBookAuthorPro.Infrastructure/  # AI providers, file system, export
│   ├── AIBookAuthorPro.UI/          # ViewModels, Views, Controls
│   └── AIBookAuthorPro.App/         # Entry point, DI configuration
└── tests/
    └── AIBookAuthorPro.Tests/       # Unit tests
```

### Technology Stack
- **.NET 8** - Modern cross-platform framework
- **WPF** - Windows Presentation Foundation
- **Material Design in XAML** - Beautiful UI components
- **CommunityToolkit.Mvvm** - MVVM source generators
- **xUnit + Moq + FluentAssertions** - Testing framework

## Getting Started

### Prerequisites
- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository:
```bash
git clone https://github.com/ncreighton/AIBookAuthorPro.git
```

2. Open the solution:
```bash
cd AIBookAuthorPro
start AIBookAuthorPro.sln
```

3. Build and run:
```bash
dotnet build
dotnet run --project src/AIBookAuthorPro.App
```

### API Keys

To use AI features, you'll need:
- **Anthropic API Key** - Get one at [console.anthropic.com](https://console.anthropic.com)
- **OpenAI API Key** (optional) - Get one at [platform.openai.com](https://platform.openai.com)

Add your keys in Settings (Ctrl+,) under the API Keys tab.

## Development

### Running Tests
```bash
dotnet test
```

### Project Structure

| Project | Description |
|---------|-------------|
| Core | Domain models, enums, interfaces, validation |
| Infrastructure | AI providers, file services, exporters |
| UI | ViewModels, Views, Controls, Converters |
| App | Entry point, dependency injection |
| Tests | Unit and integration tests |

## Roadmap

- [ ] Real-time collaboration
- [ ] Cloud sync (OneDrive, Google Drive)
- [ ] Research assistant integration
- [ ] Voice-to-text input
- [ ] Custom AI fine-tuning
- [ ] Publishing platform integrations

## Contributing

Contributions are welcome! Please read our contributing guidelines before submitting PRs.

## License

MIT License - see [LICENSE](LICENSE) for details.

## Author

**Nick Creighton**

---

*Write your story. Let AI handle the rest.*
