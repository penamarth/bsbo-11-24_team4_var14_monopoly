#!/bin/zsh -l
# Сборка и запуск тестов. Результат пишется в test_output.txt
cd -- "$(dirname -- "$0")" || exit 1
setopt pipe_fail
export PATH="$HOME/.dotnet:/usr/local/share/dotnet:/opt/homebrew/bin:/usr/local/bin:$PATH"
{
  echo "=== $(date)"
  echo "--- dotnet: $(command -v dotnet)"
  dotnet --list-sdks
  dotnet --list-runtimes
  echo "--- build"
  dotnet build
  build_status=$?
  echo "BUILD_EXIT $build_status"
  if (( build_status != 0 )); then
    exit "$build_status"
  fi

  echo "--- test"
  dotnet test --no-build --collect:"XPlat Code Coverage" --results-directory ./TestResults
  test_status=$?
  echo "TEST_EXIT $test_status"
  exit "$test_status"
} 2>&1 | tee test_output.txt
run_status=$?
echo
if (( run_status == 0 )); then
  echo "Проверка пройдена."
else
  echo "Проверка завершилась с ошибкой. Подробности в test_output.txt."
fi
exit "$run_status"
