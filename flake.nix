{
  description = "Unity 6 — C# Dev Environment";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-25.11";
    flake-utils.url = "github:numtide/flake-utils";
    git-hooks = {
      url = "github:cachix/git-hooks.nix";
      inputs.nixpkgs.follows = "nixpkgs";
    };
  };

  outputs =
    {
      self,
      nixpkgs,
      flake-utils,
      git-hooks,
      ...
    }:
    flake-utils.lib.eachDefaultSystem (
      system:
      let
        pkgs = import nixpkgs {
          inherit system;
          config.allowUnfree = true;
        };
        # ── Scripts ────────────────────────────────────────────────────────────
        scripts = pkgs.symlinkJoin {
          name = "unity-csharp-scripts";
          paths = [

            (pkgs.writeShellApplication {
              name = "check-lsp";
              text = ''
                sln=$(find . -maxdepth 1 -name "*.sln" | head -1)
                csproj=$(find . -maxdepth 2 -name "*.csproj" | head -1)

                echo ""
                echo "── LSP health check ──────────────────────"
                if [ -n "$sln" ];    then echo "✅ .sln:    $sln";
                else echo "❌ .sln ausente — regenere os arquivos de projeto no Unity."; fi
                if [ -n "$csproj" ]; then echo "✅ .csproj: $csproj";
                else echo "❌ .csproj ausente — regenere os arquivos de projeto no Unity."; fi

                echo ""
                echo "── .NET ──────────────────────────────────"
                echo "   SDK:        $(dotnet --version)"
                echo "   Root:       $DOTNET_ROOT"
                echo ""
                echo "── Ferramentas ───────────────────────────"
                echo "   csharpier:  $(dotnet csharpier --version 2>/dev/null || echo 'não encontrado')"
                echo "   netcoredbg: $(netcoredbg --version 2>/dev/null | head -1 || echo 'não encontrado')"
                echo ""
              '';
            })

            (pkgs.writeShellApplication {
              name = "restore";
              text = ''
                sln=$(find . -maxdepth 1 -name "*.sln" | head -1)
                [ -z "$sln" ] && { echo "❌ Nenhum .sln encontrado. Regenere os arquivos no Unity primeiro."; exit 1; }
                echo "📦 Restaurando pacotes NuGet para $sln..."
                dotnet restore "$sln" && echo "✅ Restaurado."
              '';
            })

            (pkgs.writeShellApplication {
              name = "build";
              text = ''
                sln=$(find . -maxdepth 1 -name "*.sln" | head -1)
                [ -z "$sln" ] && { echo "❌ Nenhum .sln encontrado. Regenere os arquivos no Unity primeiro."; exit 1; }
                config="''${1:-Debug}"
                echo "🔨 Compilando $sln [$config]..."
                dotnet build "$sln" --configuration "$config" --no-restore && echo "✅ Build concluído."
              '';
            })

            (pkgs.writeShellApplication {
              name = "fmt";
              text = ''
                echo "✨ Formatando .cs com csharpier..."
                dotnet csharpier . && echo "✅ Formatado."
              '';
            })

            (pkgs.writeShellApplication {
              name = "fmt-check";
              text = ''
                echo "🔍 Checando formatação..."
                dotnet csharpier --check . && echo "✅ Formatação OK."
              '';
            })

            (pkgs.writeShellApplication {
              name = "list-scripts";
              text = ''
                echo "📜 Scripts C# em Assets/:"
                find Assets -name "*.cs" | sort
                echo ""
                echo "Total: $(find Assets -name "*.cs" | wc -l) arquivos"
              '';
            })

            (pkgs.writeShellApplication {
              name = "clean";
              text = ''
                echo "🧹 Limpando artefatos .NET..."
                find . -maxdepth 3 \( -name "bin" -o -name "obj" \) \
                  -not -path "*/Library/*" \
                  -not -path "*/Packages/*" \
                  -exec rm -rf {} + 2>/dev/null || true
                echo "✅ Limpo."
              '';
            })

          ];
        };

        # ── Git Hooks ──────────────────────────────────────────────────────────
        hooks = git-hooks.lib.${system}.run {
          src = ./.;
          hooks = {
            # Formatation — hook customizado
            csharpier = {
              enable = true;
              name = "csharpier-format";
              description = "Checa a formatação dos scripts";
              entry = "csharpier check ./Assets/Script";
              stages = [ "pre-commit" ];
              pass_filenames = false;
            };

            no-commit-to-branch = {
              enable = true;
              settings.branch = [
                "main"
                "master"
              ];
            };
          };
        };

      in
      {
        checks.pre-commit = hooks;

        devShells.default = pkgs.mkShell {
          name = "unity-csharp";
          packages = with pkgs; [
            dotnet-sdk_9
            csharpier
            netcoredbg
            dotnet-outdated
            jq
            prek
            scripts
          ];

          env = {
            DOTNET_CLI_TELEMETRY_OPTOUT = "1";
            DOTNET_NOLOGO = "1";
            DOTNET_ROLL_FORWARD = "Major";
            NETCOREDBG_PATH = "${pkgs.netcoredbg}/bin/netcoredbg";
          };

          shellHook = hooks.shellHook + ''
            export UNITY_PROJECT_PATH="$PWD"

            echo ""
            echo "🎮 Unity 6 — C# Dev Environment"
            echo "   .NET SDK:   $(dotnet --version)"
            echo "   csharpier:  $(dotnet csharpier --version 2>/dev/null || echo 'pronto')"
            echo "   netcoredbg: $(netcoredbg --version 2>/dev/null | head -1 || echo 'pronto')"
            echo ""

            warnings=0
            if ! ls ./*.sln &>/dev/null; then
              echo "⚠️  .sln não encontrado — o LSP não vai funcionar."
              echo "   No Unity: Edit > Preferences > External Tools > Regenerate Project Files"
              warnings=$((warnings + 1))
            fi
            if [ ! -d "Assets" ]; then
              echo "⚠️  Assets/ não encontrado — você está na raiz do projeto Unity?"
              warnings=$((warnings + 1))
            fi
            [ "$warnings" -eq 0 ] && echo "✅ Projeto OK — .sln e Assets/ encontrados."

            echo ""
            echo "── Scripts ──────────────────────────────"
            echo "  check-lsp               → verifica se o LSP está OK"
            echo "  restore                 → restaura pacotes NuGet"
            echo "  build [Debug|Release]   → compila os assemblies C#"
            echo "  fmt                     → formata .cs com csharpier"
            echo "  fmt-check               → verifica formatação"
            echo "  new-script Nome [pasta] → cria MonoBehaviour com boilerplate"
            echo "  list-scripts            → lista todos os .cs em Assets/"
            echo "  clean                   → remove bin/ e obj/"
            echo ""
            echo "── LSP (roslyn.nvim) ────────────────────"
            echo "  Requer .sln na raiz. Se o LSP não indexar:"
            echo "  1. Regenere os arquivos no Unity"
            echo "  2. Rode 'restore'"
            echo "  3. Reinicie o Neovim"
            echo ""
          '';
        };
      }
    );
}
