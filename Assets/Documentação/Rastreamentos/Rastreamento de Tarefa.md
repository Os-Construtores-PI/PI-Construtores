```dataviewjs
let pages = dv.pages('"Tarefas"')
let projectPages = pages.where(p => p.file.tasks.length > 0);
let rows = projectPages.map(p => {
    let tasks = p.file.tasks;
    let completed = tasks.where(t => t.completed).length;
    let total = tasks.length;
    let percent = Math.round((completed / total) * 100) || 0;

    // Create an HTML progress bar for visual tracking
    let progressBar = `<progress value="${percent}" max="100"></progress> ${percent}%`;

    return [p.file.link, `${completed} / ${total}`, progressBar];
});
dv.table(["Tarefa", "Completada/Total)", "Barra de Progresso"], rows);
```
```dataviewjs
let pages = dv.pages('"Tarefas"');
let groups = {};

pages.forEach(p => {
    // Tags do arquivo (back-up)
    let tagsDoArquivo = p.file.etags;
    let tarefas = p.file.tasks;

    tarefas.forEach(task => {
        // 1. Verifica se a tarefa tem tags próprias. 
        // 2. Se não tiver, usa as tags do arquivo.
        let tagsParaEstaTarefa = task.tags.length > 0 ? task.tags : tagsDoArquivo;

        tagsParaEstaTarefa.forEach(tag => {
            if (!groups[tag]) {
                groups[tag] = { pending: 0, completed: 0 };
            }

            if (task.completed) {
                groups[tag].completed++;
            } else {
                groups[tag].pending++;
            }
        });
    });
});

// Transformar em linhas com cálculos
let rows = Object.entries(groups).map(([participante, data]) => {
    let total = data.pending + data.completed;
    let percent = total > 0 ? Math.round((data.completed / total) * 100) : 0;
    let progressBar = `<progress value="${percent}" max="100"></progress> ${percent}%`;

    return [
        participante,
        data.pending,
        data.completed,
        progressBar
    ];
});

// Ordenar por quem tem mais pendências
rows.sort((a, b) => b[1] - a[1]);

dv.table(["Membro da Equipe", "Pendentes", "Concluídas", "Progresso"], rows);
```

```dataviewjs
// 1. Coleta todas as páginas da pasta desejada
let pages = dv.pages('"Tarefas"');

// 2. Achata todas as tarefas de todas as páginas em uma única lista
let allTasks = pages.file.tasks;

// 3. Cálculos
let totalTasks = allTasks.length;
let completedTasks = allTasks.where(t => t.completed).length;

// Prevenção de divisão por zero e cálculo da porcentagem
let percentage = totalTasks > 0 ? Math.round((completedTasks / totalTasks) * 100) : 0;

dv.span(
  "<div style='text-align: center;'>" +
  "<h1>Progresso do Projeto<h1/>" +
  `<progress value=${percentage} max=100></progress>`+ 
  `<h1>${percentage}%<h1/>` +
  "</div>"
);

```
 































