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

        findSln = "find . -maxdepth 1 -name '*.sln' -type f -print -quit";

        scripts = pkgs.symlinkJoin {
          name = "unity-csharp-scripts";
          paths = [
            (pkgs.writeShellApplication {
              name = "check-lsp";
              text = ''
                echo "── LSP & Tool Health Check ─────────────────"
                sln=$(${findSln})
                csproj=$(find . -maxdepth 2 -name '*.csproj' -type f -print -quit)

                if [[ -n "$sln" ]]; then echo "✅ .sln:    $sln";
                else echo "❌ .sln ausente — regenere os arquivos no Unity."; fi
                if [[ -n "$csproj" ]]; then echo "✅ .csproj: $csproj";
                else echo "❌ .csproj ausente — regenere os arquivos no Unity."; fi

                echo ""
                echo "── .NET & Tools ────────────────────────────"
                echo "   SDK:        $(dotnet --version 2>/dev/null || echo 'não encontrado')"
                echo "   Root:       ''${DOTNET_ROOT:-(não definido)}"
                echo "   csharpier:  $(csharpier --version 2>/dev/null || echo 'não encontrado')"
                echo "   netcoredbg: $(command -v netcoredbg >/dev/null && echo 'instalado' || echo 'não encontrado')"
                echo ""
              '';
            })

            (pkgs.writeShellApplication {
              name = "restore";
              text = ''
                sln=$(${findSln})
                if [[ -z "$sln" ]]; then
                  echo "❌ Nenhum .sln encontrado. Regenere os arquivos no Unity primeiro."; exit 1;
                fi
                echo "📦 Restaurando pacotes NuGet para $sln..."
                dotnet restore "$sln" && echo "✅ Restaurado."
              '';
            })

            (pkgs.writeShellApplication {
              name = "build";
              text = ''
                sln=$(${findSln})
                if [[ -z "$sln" ]]; then
                  echo "❌ Nenhum .sln encontrado. Regenere os arquivos no Unity primeiro."; exit 1;
                fi
                config="''${1:-Debug}"
                echo "🔨 Compilando $sln [$config]..."
                dotnet build "$sln" --configuration "$config" --no-restore && echo "✅ Build concluído."
              '';
            })

            (pkgs.writeShellApplication {
              name = "fmt";
              text = ''
                echo "✨ Formatando .cs com csharpier..."
                csharpier . && echo "✅ Formatado."
              '';
            })

            (pkgs.writeShellApplication {
              name = "fmt-check";
              text = ''
                echo "🔍 Checando formatação..."
                csharpier check . && echo "✅ Formatação OK." || echo "❌ Formatação incorreta."
              '';
            })

            (pkgs.writeShellApplication {
              name = "list-scripts";
              text = ''
                echo "📜 Scripts C# em Assets/:"
                find Assets -name '*.cs' -type f | sort
                echo ""
                count=$(find Assets -name '*.cs' -type f | wc -l)
                echo "Total: $count arquivos"
              '';
            })

            (pkgs.writeShellApplication {
              name = "clean";
              text = ''
                echo "🧹 Limpando artefatos .NET (bin/ e obj/)..."
                find . -maxdepth 3 \( -name "bin" -o -name "obj" \) \
                  -not -path "*/Library/*" \
                  -not -path "*/Packages/*" \
                  -exec rm -rf {} + 2>/dev/null || true
                echo "✅ Limpo."
              '';
            })
          ];
        };

        hooks = git-hooks.lib.${system}.run {
          src = ./.;
          hooks = {
            csharpier = {
              enable = true;
              name = "csharpier-format";
              description = "Checa e formata scripts C# com CSharpier";
              entry = "csharpier check ./Assets/Script";
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
            prek
            csharpier
            scripts
          ];

          env = {
            DOTNET_CLI_TELEMETRY_OPTOUT = "1";
            DOTNET_NOLOGO = "1";
            DOTNET_ROLL_FORWARD = "LatestMinor";
          };

          shellHook = hooks.shellHook + ''
            export UNITY_PROJECT_PATH="$PWD"

            echo ""
            echo "🎮 Unity 6 — C# Dev Environment"
            echo "   .NET SDK:   $(dotnet --version 2>/dev/null || echo 'carregando...')"
            echo "   csharpier:  $(csharpier --version 2>/dev/null || echo 'pronto')"
            echo "   netcoredbg: $(command -v netcoredbg >/dev/null && echo 'instalado' || echo 'pronto')"
            echo ""

            warnings=0
            if ! ls ./*.sln &>/dev/null; then
              echo "⚠️  .sln não encontrado — o LSP não vai funcionar."
              echo "   No Unity: Edit > Preferences > External Tools > Regenerate Project Files"
              warnings=$((warnings + 1))
            fi
            if [[ ! -d "Assets" ]]; then
              echo "⚠️  Assets/ não encontrado — você está na raiz do projeto Unity?"
              warnings=$((warnings + 1))
            fi
            [[ "$warnings" -eq 0 ]] && echo "✅ Projeto OK — .sln e Assets/ encontrados."

            echo ""
            echo "── Scripts Disponíveis ──────────────────"
            echo "  check-lsp               → verifica saúde do LSP e ferramentas"
            echo "  restore                 → restaura pacotes NuGet"
            echo "  build [Debug|Release]   → compila assemblies C#"
            echo "  fmt                     → formata .cs com csharpier"
            echo "  fmt-check               → verifica formatação (CI-friendly)"
            echo "  list-scripts            → lista todos os .cs em Assets/"
            echo "  clean                   → remove bin/ e obj/ (exceto Library/Packages)"
            echo ""
            echo "── LSP (roslyn.nvim / Unity LSP) ──────"
            echo "  1. Certifique-se de que .sln está na raiz"
            echo "  2. Rode 'restore' se necessário"
            echo "  3. Reinicie o editor/LSP"
            echo ""
          '';
        };
      }
    );
}
