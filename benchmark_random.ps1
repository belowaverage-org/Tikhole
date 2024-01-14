$random = [System.Random]::new()
function Get-DomainList() {
    $list = [System.Collections.Generic.List[string]]::new()
    $regex = [Regex]::new("(?:^[0-9a-fA-F.:]*?\s+)([a-zA-Z0-9.-]*)(?:$)", [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $hosts_file = (Invoke-WebRequest -UseBasicParsing -Uri "https://adaway.org/hosts.txt").Content
    $collection = $regex.Matches($hosts_file)
    foreach ($match in $collection) {
        if ($match.Groups[1] -ne $null) {
            $domain = $match.Groups[1].Value
            if (-not $list.Contains($domain)) {
                $list.Add($domain)
            }
        }
    }
    return $list
}
function Start-Test($Domains) {
	$before = [DateTime]::Now.Ticks / [TimeSpan]::TicksPerMillisecond
	$_ = Resolve-DnsName -Name ($Domains[$random.Next(0, $Domains.Count - 1)]) -Server 127.0.0.1 -ErrorAction SilentlyContinue -QuickTimeout
	$after = [DateTime]::Now.Ticks / [TimeSpan]::TicksPerMillisecond
	"$($after - $before) ms"
}
$domains = Get-DomainList
while ($true) {
	Start-Test -Domains $domains
}