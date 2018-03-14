param($installPath, $toolsPath, $package, $project)

function SetPropertiesFile($projectItem)
{
    Write-Host "Properties for '"$projectItem.Name"' changed"

    # Set copy behaviour to "Copy Always"
    $copyToOutput = $projectItem.Properties.Item("CopyToOutputDirectory")
    $copyToOutput.Value = 1

    # Set build action to "None"
    $buildAction = $projectItem.Properties.Item("BuildAction")
    $buildAction.Value = 0
}

function SetPropertiesFolder($projectItem)
{
    ForEach ($item in $projectItem.ProjectItems) 
    { 
        SetPropertiesFile($item)
    } 
}

SetPropertiesFile($project.ProjectItems.Item("WeroSoft.Expressions.Library.license"))