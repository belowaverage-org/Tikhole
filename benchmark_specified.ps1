function Start-Test($Domains) {
	$before = [DateTime]::Now.Ticks / [TimeSpan]::TicksPerMillisecond
	$_ = Resolve-DnsName -Name example.com -Server 127.0.0.1 -ErrorAction SilentlyContinue -QuickTimeout
	$after = [DateTime]::Now.Ticks / [TimeSpan]::TicksPerMillisecond
	"$($after - $before) ms"
}
$domains = Get-DomainList
while ($true) {
	Start-Test -Domains $domains
}