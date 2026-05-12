{
  pkgs,
  lib,
  config,
  inputs,
  ...
}:
{
  # ── Pacotes ───────────────────────────────────────────────────────────────────
  packages = with pkgs; [
    # Formatador — necessário pro conform-nvim (cs = ["csharpier"])
    csharpier

    # Depurador — necessário pro nvim-dap com netcoredbg adapter
    netcoredbg

    # ── Utilitários de projeto ────────────────────────────────────────────────
    # Inspeciona assemblies .NET / DLLs do Unity (útil para ver APIs)
    dotnet-outdated

    # jq: usado nos scripts para parsear output do dotnet em JSON
    jq
  ];

  # ── Variáveis de ambiente ─────────────────────────────────────────────────────
  env = {
    DOTNET_CLI_TELEMETRY_OPTOUT = "1";
    DOTNET_NOLOGO = "1";
    NETCOREDBG_PATH = "${pkgs.netcoredbg}/bin/netcoredbg";
    DOTNET_ROLL_FORWARD = "Major";
    UNITY_PROJECT_PATH = "${toString ./.}";
  };

  languages = {
    dotnet = {
      enable = true;
      package = pkgs.dotnet-sdk_9;
    };
  };

  # ── Scripts ───────────────────────────────────────────────────────────────────
  scripts = {
    # ── Projeto / .sln ────────────────────────────────────────────────────────

    # Verifica se o .sln foi gerado pelo Unity (requisito para o LSP funcionar)
    check-lsp.exec = ''
      set -e
      SLN=$(find . -maxdepth 1 -name "*.sln" | head -1)
      CSPROJ=$(find . -maxdepth 2 -name "*.csproj" | head -1)

      echo ""
      echo "── LSP health check ─────────────────────"
      if [ -n "$SLN" ]; then
        echo "✅ .sln encontrado:    $SLN"
      else
        echo "❌ .sln ausente — abra o Unity e vá em:"
        echo "   Edit > Preferences > External Tools > Regenerate Project Files"
      fi

      if [ -n "$CSPROJ" ]; then
        echo "✅ .csproj encontrado: $CSPROJ"
      else
        echo "❌ .csproj ausente — regenere os arquivos de projeto no Unity."
      fi

      echo ""
      echo "── .NET ──────────────────────────────────"
      echo "   SDK:       $(dotnet --version)"
      echo "   Root:      $DOTNET_ROOT"
      echo ""
      echo "── Ferramentas ───────────────────────────"
      echo "   csharpier: $(dotnet csharpier --version 2>/dev/null || echo 'não encontrado')"
      echo "   netcoredbg: $(netcoredbg --version 2>/dev/null | head -1 || echo 'não encontrado')"
      echo ""
    '';

    # Restaura pacotes NuGet (rodar após clonar ou após Unity atualizar .csproj)
    restore.exec = ''
      set -e
      SLN=$(find . -maxdepth 1 -name "*.sln" | head -1)
      if [ -z "$SLN" ]; then
        echo "❌ Nenhum .sln encontrado. Regenere os arquivos no Unity primeiro."
        exit 1
      fi
      echo "📦 Restaurando pacotes NuGet para $SLN..."
      dotnet restore "$SLN"
      echo "✅ Restaurado."
    '';

    # Build dos assemblies C# (útil para checar erros sem abrir o Unity)
    build.exec = ''
      set -e
      SLN=$(find . -maxdepth 1 -name "*.sln" | head -1)
      if [ -z "$SLN" ]; then
        echo "❌ Nenhum .sln encontrado. Regenere os arquivos no Unity primeiro."
        exit 1
      fi
      echo "🔨 Compilando $SLN..."
      dotnet build "$SLN" --configuration "''${1:-Debug}" --no-restore
      echo "✅ Build concluído."
    '';

    # Formata todos os .cs do projeto com csharpier
    fmt.exec = ''
      echo "✨ Formatando .cs com csharpier..."
      dotnet csharpier .
      echo "✅ Formatado."
    '';

    # Checa formatação sem modificar arquivos (para CI / git hook)
    fmt-check.exec = ''
      echo "🔍 Checando formatação..."
      dotnet csharpier --check .
      echo "✅ Formatação OK."
    '';

    # Lista todos os scripts C# no Assets/
    list-scripts.exec = ''
      echo "📜 Scripts C# em Assets/:"
      find Assets -name "*.cs" | sort
      echo ""
      echo "Total: $(find Assets -name "*.cs" | wc -l) arquivos"
    '';

    # Abre um novo script com namespace e classe já preenchidos
    # Uso: new-script NomeDoScript [pasta/dentro/de/Assets]
    new-script.exec = ''
            set -e
            SCRIPT_NAME="''${1:-NewScript}"
            DEST_DIR="Assets/Scripts/''${2:-}"
            DEST_FILE="$DEST_DIR/$SCRIPT_NAME.cs"

            if [ -z "$1" ]; then
              echo "Uso: new-script NomeDoScript [subpasta/dentro/de/Assets/Scripts]"
              exit 1
            fi

            if [ -f "$DEST_FILE" ]; then
              echo "❌ Arquivo já existe: $DEST_FILE"
              exit 1
            fi

            mkdir -p "$DEST_DIR"
            cat > "$DEST_FILE" << EOF
      using UnityEngine;

      public class $SCRIPT_NAME : MonoBehaviour
      {
          private void Awake()
          {
          }

          private void Start()
          {
          }

          private void Update()
          {
          }
      }
      EOF
            echo "✅ Criado: $DEST_FILE"
            echo "⚠️  Lembre de 'Regenerate Project Files' no Unity para o LSP indexar."
    '';

    # Limpa artefatos de build do dotnet (não apaga o projeto Unity)
    clean.exec = ''
      echo "🧹 Limpando artefatos .NET..."
      find . -maxdepth 3 \( -name "bin" -o -name "obj" \) \
        -not -path "*/Library/*" \
        -not -path "*/Packages/*" \
        -exec rm -rf {} + 2>/dev/null || true
      echo "✅ Limpo."
    '';
  };

  # ── Git hooks ─────────────────────────────────────────────────────────────────
  git-hooks.hooks = {
    # Bloqueia commit se o csharpier reportar arquivos fora de formato
    csharpier = {
      enable = true;
      name = "csharpier format check";
      entry = "dotnet csharpier check";
      language = "system";
      types = [ "c#" ];
      # passa só os arquivos staged ao invés do diretório inteiro
      pass_filenames = true;
    };

    # Garante que ninguém commita direto na main
    no-commit-to-branch = {
      enable = true;
      settings.branch = [
        "main"
        "master"
      ];
    };
  };

  # ── Shell ─────────────────────────────────────────────────────────────────────
  enterShell = ''
    echo ""
    echo "🎮 Unity 6 — C# Dev Environment"
    echo "   .NET SDK:   $(dotnet --version)"
    echo "   csharpier:  $(dotnet csharpier --version 2>/dev/null || echo 'pronto')"
    echo "   netcoredbg: $(netcoredbg --version 2>/dev/null | head -1 || echo 'pronto')"
    echo ""

    # ── Avisos de saúde do projeto ───────────────────────────────────────────
    WARNINGS=0

    if ! ls *.sln &>/dev/null 2>&1; then
      echo "⚠️  .sln não encontrado — o LSP (roslyn.nvim) não vai funcionar."
      echo "   No Unity: Edit > Preferences > External Tools > Regenerate Project Files"
      WARNINGS=$((WARNINGS + 1))
    fi

    if [ ! -d "Assets" ]; then
      echo "⚠️  Pasta Assets/ não encontrada — você está na raiz do projeto Unity?"
      WARNINGS=$((WARNINGS + 1))
    fi

    if [ $WARNINGS -eq 0 ]; then
      echo "✅ Projeto OK — .sln e Assets/ encontrados."
    fi

    echo ""
    echo "── Scripts ─────────────────────────────"
    echo "  check-lsp       → verifica se o LSP está configurado corretamente"
    echo "  restore         → restaura pacotes NuGet"
    echo "  build [config]  → compila os assemblies C# (Debug ou Release)"
    echo "  fmt             → formata .cs com csharpier"
    echo "  fmt-check       → verifica formatação (sem modificar)"
    echo "  new-script Nome [pasta] → cria MonoBehaviour com boilerplate"
    echo "  list-scripts    → lista todos os .cs em Assets/"
    echo "  clean           → remove bin/ e obj/ do dotnet"
    echo ""
    echo "── LSP (roslyn.nvim) ───────────────────"
    echo "  Requer .sln na raiz. Se o LSP não indexar após abrir um .cs:"
    echo "  1. Regenere os arquivos de projeto no Unity"
    echo "  2. Rode 'restore' aqui no shell"
    echo "  3. Reinicie o Neovim"
    echo ""
  '';
}
